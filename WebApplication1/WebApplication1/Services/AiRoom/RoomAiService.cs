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
    IOpenAiService openAiService) : IRoomAiService
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

        var roomDesign = await context.RoomDesigns
            .Include(x => x.RoomUpload)
            .Include(x => x.RoomFurniturePlacements)
                .ThenInclude(x => x.Product)
                    .ThenInclude(x => x.ProductImages)
            .FirstOrDefaultAsync(x => x.Id == request.RoomDesignId, cancellationToken)
            ?? throw new NotFoundException($"Room design '{request.RoomDesignId}' was not found.");

        foreach (var placement in roomDesign.RoomFurniturePlacements)
        {
            await EnsureProductIsInCartAsync(userId, placement.ProductId, cancellationToken);
        }

        var promptData = new RoomAiPromptData
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

        var prompt = BuildPrompt(promptData);
        var aiResult = await openAiService.GenerateRealisticRoomAsync(prompt, promptData, cancellationToken);
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

    private static string BuildPrompt(RoomAiPromptData data)
    {
        var placementLines = data.Products.Select(product =>
            $"- Product: {product.Name}\n  ProductId: {product.ProductId}\n  ImageUrl: {product.ImageUrl}\n  PositionX: {product.PositionX}\n  PositionY: {product.PositionY}\n  Rotation: {product.Rotation}\n  Scale: {product.Scale}\n  ZIndex: {product.ZIndex}");

        return "Use the uploaded empty room image as the base. Keep the same walls, floor, lighting, windows, camera angle, and perspective. " +
               "Add the furniture items based on the placement data. Make the result realistic and proportional. " +
               "Do not add furniture that is not listed. Do not change the room structure.\n\n" +
               $"Room image: {data.RoomImageUrl}\n" +
               $"Room type: {data.RoomType}\n" +
               $"User dimensions: width={data.Width}, height={data.Height}, depth={data.Depth}. Treat AI dimensions as approximate and prefer user dimensions when provided.\n\n" +
               "Placements:\n" + string.Join("\n", placementLines);
    }
}
