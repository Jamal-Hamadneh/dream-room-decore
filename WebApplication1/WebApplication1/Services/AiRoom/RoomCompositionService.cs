using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace WebApplication1.Services;

public class RoomCompositionService(HttpClient httpClient) : IRoomCompositionService
{
    private const int CanvasWidth = 800;
    private const int CanvasHeight = 600;
    private const int BaseFurnitureSize = 220;

    public async Task<byte[]> ComposeRoomPreviewAsync(RoomAiPromptData data, CancellationToken cancellationToken = default)
    {
        using var canvas = await LoadImageAsync(data.RoomImageUrl, cancellationToken);
        canvas.Mutate(ctx => ctx.Resize(CanvasWidth, CanvasHeight));

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
                RemoveLightBackground(productImage);

                var scale = product.Scale > 0 ? (double)product.Scale : 1d;
                var targetWidth = Math.Max(1, (int)Math.Round(BaseFurnitureSize * scale));
                var targetHeight = Math.Max(1, (int)Math.Round(productImage.Height * (targetWidth / (double)productImage.Width)));

                productImage.Mutate(ctx => ctx.Resize(targetWidth, targetHeight));
                ApplyEdgeFeather(productImage);
                productImage.Mutate(ctx => ctx.Rotate((float)product.Rotation));

                var x = (int)product.PositionX - productImage.Width / 2;
                var y = (int)product.PositionY - productImage.Height / 2;

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

    // Removes flat near-white studio backdrops so pasted products don't look like framed photos.
    private static void RemoveLightBackground(Image<Rgba32> image)
    {
        const byte threshold = 235;
        const byte fadeRange = 15;

        image.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (var x = 0; x < row.Length; x++)
                {
                    ref var pixel = ref row[x];
                    var min = Math.Min(pixel.R, Math.Min(pixel.G, pixel.B));

                    if (min >= threshold)
                    {
                        pixel.A = 0;
                    }
                    else if (min >= threshold - fadeRange)
                    {
                        var fade = (double)(min - (threshold - fadeRange)) / fadeRange;
                        pixel.A = (byte)(pixel.A * (1 - fade));
                    }
                }
            }
        });
    }

    // Fades the rectangular edges of pasted product photos to transparent (elliptical vignette),
    // so gradient studio backdrops don't read as a "framed photo" pasted onto the room.
    private static void ApplyEdgeFeather(Image<Rgba32> image)
    {
        const double innerRadius = 0.7;
        const double outerRadius = 1.0;

        var centerX = (image.Width - 1) / 2.0;
        var centerY = (image.Height - 1) / 2.0;

        image.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                var ny = centerY == 0 ? 0 : (y - centerY) / centerY;
                for (var x = 0; x < row.Length; x++)
                {
                    var nx = centerX == 0 ? 0 : (x - centerX) / centerX;
                    var distance = Math.Sqrt(nx * nx + ny * ny);

                    if (distance >= outerRadius)
                    {
                        row[x].A = 0;
                    }
                    else if (distance > innerRadius)
                    {
                        var fade = (distance - innerRadius) / (outerRadius - innerRadius);
                        row[x].A = (byte)(row[x].A * (1 - fade));
                    }
                }
            }
        });
    }
}
