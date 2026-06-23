using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace WebApplication1.Services;

public class RoomCompositionService(HttpClient httpClient) : IRoomCompositionService
{
    // 3:2 landscape that matches a native gpt-image-1 output size (1536x1024), so the
    // photorealistic render step never has to stretch or letterbox the composite.
    private const int CanvasWidth = 1536;
    private const int CanvasHeight = 1024;

    // A furniture item at scale 1 occupies this fraction of the canvas width. Expressed as a
    // fraction (not an absolute pixel size) so the browser preview can reproduce the exact same
    // layout at any display size, keeping the preview and the generated image consistent.
    private const double BaseFurnitureFraction = 0.25;

    public async Task<byte[]> ComposeRoomImageAsync(RoomAiPromptData data, CancellationToken cancellationToken = default)
    {
        using var canvas = await LoadImageAsync(data.RoomImageUrl, cancellationToken);
        canvas.Mutate(ctx => ctx.Resize(new ResizeOptions
        {
            Size = new Size(CanvasWidth, CanvasHeight),
            Mode = ResizeMode.Crop,
            Position = AnchorPositionMode.Center
        }));

        var ambient = SampleAmbientLight(canvas);

        foreach (var product in data.Products.OrderBy(x => x.ZIndex))
        {
            if (string.IsNullOrWhiteSpace(product.ImageUrl))
            {
                continue;
            }

            Image<Rgba32>? productImage;
            try
            {
                productImage = await LoadImageAsync(product.ImageUrl, cancellationToken);
            }
            catch (HttpRequestException)
            {
                continue;
            }

            using (productImage)
            {
                RemoveStudioBackground(productImage);

                var scale = product.Scale > 0 ? (double)product.Scale : 1d;
                var targetWidth = Math.Max(1, (int)Math.Round(CanvasWidth * BaseFurnitureFraction * scale));
                var targetHeight = Math.Max(1, (int)Math.Round(productImage.Height * (targetWidth / (double)productImage.Width)));

                productImage.Mutate(ctx => ctx.Resize(targetWidth, targetHeight));
                SmoothCutoutEdges(productImage);
                HarmonizeLighting(productImage, ambient);
                productImage.Mutate(ctx => ctx.Rotate((float)product.Rotation));

                // PositionX/PositionY are 0..1 fractions marking the furniture's center on the
                // canvas, so the same values render identically in the browser preview.
                var centerX = (int)Math.Round((double)product.PositionX * CanvasWidth);
                var centerY = (int)Math.Round((double)product.PositionY * CanvasHeight);
                var x = centerX - productImage.Width / 2;
                var y = centerY - productImage.Height / 2;

                using (var castShadow = CreateCastShadow(productImage))
                {
                    var offsetX = ambient.ShadowDirectionX * (int)Math.Round(productImage.Width * 0.08);
                    var offsetY = (int)Math.Round(productImage.Height * 0.12);
                    canvas.Mutate(ctx => ctx.DrawImage(castShadow, new Point(x + offsetX, y + offsetY), 0.35f));
                }

                using (var contactShadow = CreateContactShadow(productImage))
                {
                    var offsetY = productImage.Height - contactShadow.Height / 2;
                    canvas.Mutate(ctx => ctx.DrawImage(contactShadow, new Point(x, y + offsetY), 0.45f));
                }

                canvas.Mutate(ctx => ctx.DrawImage(productImage, new Point(x, y), 1f));
            }
        }

        using var stream = new MemoryStream();
        await canvas.SaveAsPngAsync(stream, cancellationToken);
        return stream.ToArray();
    }

    private async Task<Image<Rgba32>> LoadImageAsync(string url, CancellationToken cancellationToken)
    {
        var bytes = await httpClient.GetByteArrayAsync(url, cancellationToken);
        return Image.Load<Rgba32>(bytes);
    }

    private readonly record struct AmbientLight(byte R, byte G, byte B, int ShadowDirectionX);

    // Samples the room canvas's average wall/floor color and estimates which side the light
    // comes from (the brighter half), so products can be tinted and shadowed to match.
    private static AmbientLight SampleAmbientLight(Image<Rgba32> canvas)
    {
        long sumR = 0, sumG = 0, sumB = 0, count = 0;
        var quadrantBrightness = new double[4];
        var quadrantCount = new long[4];

        canvas.ProcessPixelRows(accessor =>
        {
            var width = accessor.Width;
            var height = accessor.Height;
            var midX = width / 2;
            var midY = height / 2;

            for (var y = 0; y < height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (var x = 0; x < width; x++)
                {
                    var pixel = row[x];
                    sumR += pixel.R;
                    sumG += pixel.G;
                    sumB += pixel.B;
                    count++;

                    var quadrant = (x < midX ? 0 : 1) + (y < midY ? 0 : 2);
                    quadrantBrightness[quadrant] += (pixel.R + pixel.G + pixel.B) / 3.0;
                    quadrantCount[quadrant]++;
                }
            }
        });

        var brightestQuadrant = 0;
        for (var i = 1; i < 4; i++)
        {
            var avg = quadrantCount[i] > 0 ? quadrantBrightness[i] / quadrantCount[i] : 0;
            var bestAvg = quadrantCount[brightestQuadrant] > 0 ? quadrantBrightness[brightestQuadrant] / quadrantCount[brightestQuadrant] : 0;
            if (avg > bestAvg)
            {
                brightestQuadrant = i;
            }
        }

        var lightFromRight = brightestQuadrant is 1 or 3;
        var shadowDirectionX = lightFromRight ? -1 : 1;

        return new AmbientLight(
            (byte)(sumR / count),
            (byte)(sumG / count),
            (byte)(sumB / count),
            shadowDirectionX);
    }

    // Subtly shifts a product's color balance toward the room's ambient wall/floor tone so it
    // reads as lit by the same light as the room, without changing its base color or material.
    private static void HarmonizeLighting(Image<Rgba32> product, AmbientLight ambient)
    {
        long sumR = 0, sumG = 0, sumB = 0, count = 0;

        product.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (var x = 0; x < row.Length; x++)
                {
                    var pixel = row[x];
                    if (pixel.A == 0)
                    {
                        continue;
                    }

                    sumR += pixel.R;
                    sumG += pixel.G;
                    sumB += pixel.B;
                    count++;
                }
            }
        });

        if (count == 0)
        {
            return;
        }

        const double strength = 0.18;
        var factorR = Math.Clamp(GetBlendFactor((double)sumR / count, ambient.R, strength), 0.85, 1.15);
        var factorG = Math.Clamp(GetBlendFactor((double)sumG / count, ambient.G, strength), 0.85, 1.15);
        var factorB = Math.Clamp(GetBlendFactor((double)sumB / count, ambient.B, strength), 0.85, 1.15);

        product.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (var x = 0; x < row.Length; x++)
                {
                    var pixel = row[x];
                    if (pixel.A == 0)
                    {
                        continue;
                    }

                    row[x] = new Rgba32(
                        (byte)Math.Clamp(pixel.R * factorR, 0, 255),
                        (byte)Math.Clamp(pixel.G * factorG, 0, 255),
                        (byte)Math.Clamp(pixel.B * factorB, 0, 255),
                        pixel.A);
                }
            }
        });
    }

    private static double GetBlendFactor(double current, double target, double strength)
    {
        if (current <= 0)
        {
            return 1;
        }

        var blended = current + (target - current) * strength;
        return blended / current;
    }

    private static readonly (int Dx, int Dy)[] FloodNeighbors =
    [
        (1, 0), (-1, 0), (0, 1), (0, -1)
    ];

    // Removes the white/near-white studio backdrop around a product photo by flood-filling from
    // the image borders. Product photos are generated with a plain white background, so any
    // near-white pixel reachable from the border is background - this fixed threshold (rather
    // than a color range sampled from the border) avoids the fill drifting onto the product
    // itself even when the product silhouette touches the image edge.
    private static void RemoveStudioBackground(Image<Rgba32> image)
    {
        const int whiteThreshold = 240;

        image.ProcessPixelRows(accessor =>
        {
            var width = accessor.Width;
            var height = accessor.Height;
            var visited = new bool[width * height];
            var queue = new Queue<(int X, int Y)>();
            var borderPoints = new List<(int X, int Y)>();

            for (var x = 0; x < width; x++)
            {
                borderPoints.Add((x, 0));
                borderPoints.Add((x, height - 1));
            }

            for (var y = 0; y < height; y++)
            {
                borderPoints.Add((0, y));
                borderPoints.Add((width - 1, y));
            }

            bool IsBackground(Rgba32 pixel) =>
                pixel.A == 0 ||
                (pixel.R >= whiteThreshold && pixel.G >= whiteThreshold && pixel.B >= whiteThreshold);

            foreach (var (x, y) in borderPoints)
            {
                var index = y * width + x;
                if (visited[index])
                {
                    continue;
                }

                if (!IsBackground(accessor.GetRowSpan(y)[x]))
                {
                    continue;
                }

                visited[index] = true;
                queue.Enqueue((x, y));
            }

            while (queue.Count > 0)
            {
                var (x, y) = queue.Dequeue();
                var row = accessor.GetRowSpan(y);
                row[x] = row[x] with { A = 0 };

                foreach (var (dx, dy) in FloodNeighbors)
                {
                    var nx = x + dx;
                    var ny = y + dy;
                    if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                    {
                        continue;
                    }

                    var neighborIndex = ny * width + nx;
                    if (visited[neighborIndex])
                    {
                        continue;
                    }

                    var neighbor = accessor.GetRowSpan(ny)[nx];
                    if (IsBackground(neighbor))
                    {
                        visited[neighborIndex] = true;
                        queue.Enqueue((nx, ny));
                    }
                }
            }
        });
    }

    // Slightly blurs the cutout silhouette so the flood-filled edge anti-aliases into the
    // room instead of leaving a hard, jagged outline.
    private static void SmoothCutoutEdges(Image<Rgba32> image)
    {
        image.Mutate(ctx => ctx.GaussianBlur(0.6f));
    }

    // Soft, blurred silhouette cast away from the room's estimated light source so each item
    // grounds itself in the scene instead of floating above the floor.
    private static Image<Rgba32> CreateCastShadow(Image<Rgba32> source)
    {
        var shadow = ToSilhouette(source);

        shadow.Mutate(ctx => ctx
            .GaussianBlur(10f)
            .Resize(new ResizeOptions
            {
                Size = new Size(shadow.Width, Math.Max(1, (int)Math.Round(shadow.Height * 0.5))),
                Mode = ResizeMode.Stretch
            }));

        return shadow;
    }

    // Tight, darker shadow directly beneath the object's base for contact/ambient occlusion.
    private static Image<Rgba32> CreateContactShadow(Image<Rgba32> source)
    {
        var shadow = ToSilhouette(source);

        shadow.Mutate(ctx => ctx
            .GaussianBlur(3f)
            .Resize(new ResizeOptions
            {
                Size = new Size(shadow.Width, Math.Max(1, (int)Math.Round(shadow.Height * 0.14))),
                Mode = ResizeMode.Stretch
            }));

        return shadow;
    }

    private static Image<Rgba32> ToSilhouette(Image<Rgba32> source)
    {
        var silhouette = source.Clone();
        silhouette.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (var x = 0; x < row.Length; x++)
                {
                    var alpha = row[x].A;
                    row[x] = alpha == 0 ? default : new Rgba32(0, 0, 0, alpha);
                }
            }
        });

        return silhouette;
    }
}
