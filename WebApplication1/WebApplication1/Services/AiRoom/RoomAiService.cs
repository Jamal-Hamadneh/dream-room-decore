using Microsoft.EntityFrameworkCore;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Data;
using WebApplication1.Exceptions;
using WebApplication1.Exceptions.AiRoom;
using WebApplication1.Models;

namespace WebApplication1.Services;

public class RoomAiService(
    ApplicationDbContext context,
    ICloudinaryService cloudinaryService,
    IOpenAiService openAiService,
    IRoomCompositionService roomCompositionService) : IRoomAiService
{
    public async Task<UploadAndCreateDesignResponse> UploadAndCreateDesignAsync(int userId, UploadAndCreateDesignRequest request, CancellationToken cancellationToken = default)
    {
        var cartProducts = await GetCartProductsAsync(userId, cancellationToken);
        if (cartProducts.Count == 0)
        {
            throw new CartMustHaveFurnitureException();
        }

        var imageUrl = await cloudinaryService.UploadImageAsync(request.RoomImage, "furniture/rooms", cancellationToken);
        var now = DateTime.UtcNow;

        var roomUpload = new RoomUpload
        {
            UserId = userId,
            ImageUrl = imageUrl,
            RoomType = request.RoomType ?? string.Empty,
            Height = request.Height ?? 0,
            Width = request.Width ?? 0,
            Depth = request.Depth ?? 0,
            CreatedAt = now
        };

        context.RoomUploads.Add(roomUpload);
        await context.SaveChangesAsync(cancellationToken);

        var roomDesign = new RoomDesign
        {
            RoomUploadId = roomUpload.Id,
            Name = $"{(string.IsNullOrWhiteSpace(roomUpload.RoomType) ? "Room" : roomUpload.RoomType)} Design",
            CreatedAt = now
        };

        context.RoomDesigns.Add(roomDesign);
        await context.SaveChangesAsync(cancellationToken);

        return new UploadAndCreateDesignResponse
        {
            RoomUploadId = roomUpload.Id,
            RoomDesignId = roomDesign.Id,
            ImageUrl = imageUrl,
            CartProducts = cartProducts
        };
    }

    public async Task<PlacementResponse> SavePlacementAsync(int userId, SavePlacementRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureRoomDesignBelongsToUserAsync(userId, request.RoomDesignId, cancellationToken);
        await EnsureProductIsInCartAsync(userId, request.ProductId, cancellationToken);

        var placement = await context.RoomFurniturePlacements
            .FirstOrDefaultAsync(x => x.RoomDesignId == request.RoomDesignId && x.ProductId == request.ProductId, cancellationToken);

        if (placement is null)
        {
            placement = new RoomFurniturePlacement
            {
                RoomDesignId = request.RoomDesignId,
                ProductId = request.ProductId,
                CreatedAt = DateTime.UtcNow
            };
            context.RoomFurniturePlacements.Add(placement);
        }

        ApplyPlacementValues(placement, request.PositionX, request.PositionY, request.Rotation, request.Scale, request.ZIndex);
        await context.SaveChangesAsync(cancellationToken);
        return ToPlacementResponse(placement);
    }

    public async Task<PlacementResponse> UpdatePlacementAsync(int userId, UpdatePlacementRequest request, CancellationToken cancellationToken = default)
    {
        var placement = await context.RoomFurniturePlacements.FirstOrDefaultAsync(x => x.Id == request.PlacementId, cancellationToken)
            ?? throw new NotFoundException($"Placement '{request.PlacementId}' was not found.");

        await EnsureRoomDesignBelongsToUserAsync(userId, placement.RoomDesignId, cancellationToken);
        await EnsureProductIsInCartAsync(userId, placement.ProductId, cancellationToken);

        ApplyPlacementValues(placement, request.PositionX, request.PositionY, request.Rotation, request.Scale, request.ZIndex);
        await context.SaveChangesAsync(cancellationToken);
        return ToPlacementResponse(placement);
    }

    public async Task<PlacementResponse> SwitchProductAsync(int userId, SwitchProductRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureRoomDesignBelongsToUserAsync(userId, request.RoomDesignId, cancellationToken);
        await EnsureProductIsInCartAsync(userId, request.NewProductId, cancellationToken);

        var placement = await context.RoomFurniturePlacements
            .FirstOrDefaultAsync(x => x.RoomDesignId == request.RoomDesignId && x.ProductId == request.OldProductId, cancellationToken)
            ?? throw new NotFoundException($"Placement for product '{request.OldProductId}' was not found.");

        placement.ProductId = request.NewProductId;
        placement.UpdatedAt = DateTime.UtcNow;

        context.RoomDesignReplacements.Add(new RoomDesignReplacement
        {
            RoomDesignId = request.RoomDesignId,
            OldProductId = request.OldProductId,
            NewProductId = request.NewProductId,
            Instruction = $"Replace product {request.OldProductId} with product {request.NewProductId} and keep the same placement.",
            Status = "completed",
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync(cancellationToken);
        return ToPlacementResponse(placement);
    }

    public async Task<GenerateRealisticDesignResponse> GenerateRealisticDesignAsync(int userId, GenerateRealisticDesignRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureRoomDesignBelongsToUserAsync(userId, request.RoomDesignId, cancellationToken);
        var roomDesign = await GetRoomDesignForGenerationAsync(request.RoomDesignId, cancellationToken);
        var promptData = await BuildPromptDataAsync(userId, roomDesign, cancellationToken);

        var previewImageBytes = await roomCompositionService.ComposeRoomPreviewAsync(promptData, cancellationToken);

        await using var uploadStream = new MemoryStream(previewImageBytes);
        var previewImageUrl = await cloudinaryService.UploadImageStreamAsync(uploadStream, "preview.png", "furniture/previews", cancellationToken);
        promptData.PreviewImageUrl = previewImageUrl;

        var prompt = BuildPreviewPrompt(promptData);
        await using var editStream = new MemoryStream(previewImageBytes);
        var aiResult = await openAiService.GenerateRealisticRoomFromPreviewAsync(prompt, promptData, editStream, "image/png", "preview.png", cancellationToken);

        return await SaveGeneratedImageAsync(roomDesign, prompt, aiResult, cancellationToken);
    }

    public async Task<GenerateRealisticDesignResponse> GenerateRealisticDesignFromPreviewAsync(int userId, GenerateRealisticDesignFromPreviewRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureRoomDesignBelongsToUserAsync(userId, request.RoomDesignId, cancellationToken);
        var roomDesign = await GetRoomDesignForGenerationAsync(request.RoomDesignId, cancellationToken);
        var previewImageUrl = await cloudinaryService.UploadImageAsync(request.PreviewImage, "furniture/previews", cancellationToken);
        var promptData = await BuildPromptDataAsync(userId, roomDesign, cancellationToken);
        promptData.PreviewImageUrl = previewImageUrl;
        var prompt = BuildPreviewPrompt(promptData);

        await using var previewStream = request.PreviewImage.OpenReadStream();
        var aiResult = await openAiService.GenerateRealisticRoomFromPreviewAsync(prompt, promptData, previewStream, request.PreviewImage.ContentType, request.PreviewImage.FileName, cancellationToken);

        return await SaveGeneratedImageAsync(roomDesign, prompt, aiResult, cancellationToken);
    }

    private async Task<RoomDesign> GetRoomDesignForGenerationAsync(int roomDesignId, CancellationToken cancellationToken)
    {
        return await context.RoomDesigns
            .Include(x => x.RoomUpload)
            .Include(x => x.RoomFurniturePlacements)
                .ThenInclude(x => x.Product)
                    .ThenInclude(x => x.ProductImages)
            .FirstOrDefaultAsync(x => x.Id == roomDesignId, cancellationToken)
            ?? throw new NotFoundException($"Room design '{roomDesignId}' was not found.");
    }

    private async Task<RoomAiPromptData> BuildPromptDataAsync(int userId, RoomDesign roomDesign, CancellationToken cancellationToken)
    {
        foreach (var placement in roomDesign.RoomFurniturePlacements)
        {
            await EnsureProductIsInCartAsync(userId, placement.ProductId, cancellationToken);
        }

        return new RoomAiPromptData
        {
            RoomImageUrl = roomDesign.RoomUpload.ImageUrl,
            RoomType = roomDesign.RoomUpload.RoomType,
            Height = roomDesign.RoomUpload.Height,
            Width = roomDesign.RoomUpload.Width,
            Depth = roomDesign.RoomUpload.Depth,
            Products = roomDesign.RoomFurniturePlacements
                .OrderBy(x => x.ZIndex)
                .Select(x => new RoomAiProductData
                {
                    ProductId = x.ProductId,
                    Name = x.Product.Name,
                    Description = x.Product.Description,
                    Material = x.Product.Material,
                    Color = x.Product.Color,
                    ImageUrl = x.Product.ProductImages.OrderByDescending(image => image.IsMain).FirstOrDefault()?.ImageUrl,
                    PositionX = x.PositionX,
                    PositionY = x.PositionY,
                    Rotation = x.Rotation,
                    Scale = x.Scale,
                    ZIndex = x.ZIndex
                })
                .ToList()
        };
    }

    private async Task<GenerateRealisticDesignResponse> SaveGeneratedImageAsync(RoomDesign roomDesign, string prompt, OpenAiRoomResult aiResult, CancellationToken cancellationToken)
    {
        var generatedUrl = !string.IsNullOrWhiteSpace(aiResult.GeneratedImageDataUri)
            ? await cloudinaryService.UploadImageDataUriAsync(aiResult.GeneratedImageDataUri, "furniture/generated", cancellationToken)
            : await cloudinaryService.UploadImageFromUrlAsync(aiResult.GeneratedImageSourceUrl, "furniture/generated", cancellationToken);

        roomDesign.RoomUpload.AiDescription = aiResult.AnalysisJson;

        var generatedRoomImage = new GeneratedRoomImage
        {
            RoomDesignId = roomDesign.Id,
            GeneratedImageUrl = generatedUrl,
            Prompt = prompt,
            AiAnalysisJson = aiResult.AnalysisJson,
            Status = "completed",
            CreatedAt = DateTime.UtcNow
        };

        context.GeneratedRoomImages.Add(generatedRoomImage);
        await context.SaveChangesAsync(cancellationToken);

        return new GenerateRealisticDesignResponse
        {
            GeneratedRoomImageId = generatedRoomImage.Id,
            GeneratedImageUrl = generatedUrl,
            AiAnalysisJson = aiResult.AnalysisJson
        };
    }

    private async Task EnsureRoomDesignBelongsToUserAsync(int userId, int roomDesignId, CancellationToken cancellationToken)
    {
        var belongsToUser = await context.RoomDesigns
            .AnyAsync(design => design.Id == roomDesignId && design.RoomUpload.UserId == userId, cancellationToken);

        if (!belongsToUser)
        {
            throw new RoomDesignAccessDeniedException(roomDesignId);
        }
    }

    private async Task EnsureProductIsInCartAsync(int userId, int productId, CancellationToken cancellationToken)
    {
        var isInCart = await context.CartItems
            .AnyAsync(item => item.Cart.UserId == userId && item.ProductId == productId, cancellationToken);

        if (!isInCart)
        {
            throw new ProductMustBeInCartException(productId);
        }
    }

    private async Task<List<AiRoomProductResponse>> GetCartProductsAsync(int userId, CancellationToken cancellationToken)
    {
        var cart = await context.Carts
            .Include(x => x.CartItems)
                .ThenInclude(x => x.Product)
                    .ThenInclude(x => x.ProductImages)
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (cart is null)
        {
            return new List<AiRoomProductResponse>();
        }

        return cart.CartItems.Select(item => new AiRoomProductResponse
        {
            ProductId = item.ProductId,
            Name = item.Product.Name,
            Description = item.Product.Description,
            Material = item.Product.Material,
            Color = item.Product.Color,
            Height = item.Product.Height,
            Width = item.Product.Width,
            Depth = item.Product.Depth,
            Quantity = item.Quantity,
            ImageUrl = item.Product.ProductImages.OrderByDescending(image => image.IsMain).FirstOrDefault()?.ImageUrl
        }).ToList();
    }

    private static void ApplyPlacementValues(RoomFurniturePlacement placement, decimal positionX, decimal positionY, decimal rotation, decimal scale, int zIndex)
    {
        placement.PositionX = positionX;
        placement.PositionY = positionY;
        placement.Rotation = rotation;
        placement.Scale = scale;
        placement.ZIndex = zIndex;
        placement.UpdatedAt = DateTime.UtcNow;
    }

    private static PlacementResponse ToPlacementResponse(RoomFurniturePlacement placement)
    {
        return new PlacementResponse
        {
            Id = placement.Id,
            RoomDesignId = placement.RoomDesignId,
            ProductId = placement.ProductId,
            PositionX = placement.PositionX,
            PositionY = placement.PositionY,
            Rotation = placement.Rotation,
            Scale = placement.Scale,
            ZIndex = placement.ZIndex
        };
    }

    private static string BuildPreviewPrompt(RoomAiPromptData data)
    {
        var productLines = data.Products.Select(product =>
            $"- {product.Name}: current color {product.Color}, material {product.Material}, product image {product.ImageUrl}, zIndex {product.ZIndex}");
        var productNames = string.Join(", ", data.Products.Select(product => product.Name));

        return "Use the provided composed preview image as the exact layout reference and as the exact visual reference for every product. " +
               "Keep every visible product on the same side, in the same approximate position, scale, rotation, and layer order as the preview. " +
               "Do not move furniture to another side of the room. " +
               "Preserve the exact color, material, texture, and shape of every product exactly as shown in the preview image - do not recolor, restyle, or redesign any product. " +
               "Preserve the room's wall color, floor, lighting style, and camera angle exactly as shown in the preview image. " +
               "The only allowed change is to make the pasted product photos blend naturally into the room (correct perspective, add realistic shadows and reflections, smooth edges) without altering their colors or materials. " +
               $"Allowed products only: {productNames}. Exact product count: {data.Products.Count}. " +
               "Do not add any product or decor that is not visible in the preview. Do not add plants, rugs, shelves, wall art, chairs, tables, lamps, pillows, books, or accessories unless they are one of the allowed products listed below. " +
               "If an area is empty in the preview, keep it empty. Do not change the room structure. " +
               "In the JSON analysis, include colorRecommendations with palette, productColorAdvice, wallColorAdvice, and whyTheseColorsWork.\n\n" +
               $"Original room image: {data.RoomImageUrl}\n" +
               $"Composed preview image: {data.PreviewImageUrl}\n" +
               $"Room type: {data.RoomType}\n" +
               $"User dimensions: width={data.Width}, height={data.Height}, depth={data.Depth}.\n\n" +
               "Allowed products:\n" + string.Join("\n", productLines);
    }
}
