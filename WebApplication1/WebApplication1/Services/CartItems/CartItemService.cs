using Microsoft.EntityFrameworkCore;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Data;
using WebApplication1.Exceptions;
using WebApplication1.Exceptions.Products;
using WebApplication1.Exceptions.ProductVariants;
using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Services;

public interface ICartItemService : ICrudService<CartItemRequest, CartItemResponse>;

public class CartItemService(ICartItemRepository repository, ICrudMapper<CartItem, CartItemRequest, CartItemResponse> mapper, ApplicationDbContext context)
    : CrudService<CartItem, CartItemRequest, CartItemResponse>(repository, mapper), ICartItemService
{
    public override async Task<List<CartItemResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await Query().ToListAsync(cancellationToken);
        return items.Select(ToResponse).ToList();
    }

    public override async Task<CartItemResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await Query().FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        return item is null ? null : ToResponse(item);
    }

    public override async Task<CartItemResponse> CreateAsync(CartItemRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateProductAvailabilityAsync(request.ProductId, request.ProductVariantId, request.Quantity, cancellationToken);
        var response = await base.CreateAsync(request, cancellationToken);
        return await GetByIdAsync(response.Id, cancellationToken) ?? response;
    }

    public override async Task<CartItemResponse?> UpdateAsync(int id, CartItemRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateProductAvailabilityAsync(request.ProductId, request.ProductVariantId, request.Quantity, cancellationToken);
        var response = await base.UpdateAsync(id, request, cancellationToken);
        return response is null ? null : await GetByIdAsync(id, cancellationToken);
    }

    private async Task ValidateProductAvailabilityAsync(int productId, int? variantId, int quantity, CancellationToken cancellationToken)
    {
        var product = await context.Products.AsNoTracking().FirstOrDefaultAsync(product => product.Id == productId, cancellationToken)
            ?? throw new NotFoundException($"Product '{productId}' was not found.");

        if (!product.IsActive)
        {
            throw new ProductIsInactiveException(productId);
        }

        if (variantId.HasValue)
        {
            var variant = await context.ProductVariants.AsNoTracking().FirstOrDefaultAsync(variant => variant.Id == variantId.Value, cancellationToken)
                ?? throw new NotFoundException($"Product variant '{variantId}' was not found.");

            if (variant.ProductId != productId)
            {
                throw new ProductVariantDoesNotBelongToProductException(variant.Id, productId);
            }

            if (variant.StockQuantity < quantity)
            {
                throw new InsufficientProductStockException(productId, quantity, variant.StockQuantity);
            }

            return;
        }

        if (product.StockQuantity < quantity)
        {
            throw new InsufficientProductStockException(productId, quantity, product.StockQuantity);
        }
    }

    private IQueryable<CartItem> Query() => context.CartItems
        .AsNoTracking()
        .Include(item => item.Product)
            .ThenInclude(product => product.ProductImages)
        .Include(item => item.ProductVariant);

    private static CartItemResponse ToResponse(CartItem item)
    {
        var unitPrice = item.ProductVariant?.Price ?? item.Product.DiscountPrice ?? item.Product.Price;

        return new CartItemResponse
        {
            Id = item.Id,
            CartId = item.CartId,
            ProductId = item.ProductId,
            ProductVariantId = item.ProductVariantId,
            Quantity = item.Quantity,
            CreatedAt = item.CreatedAt,
            Product = ResponseMapping.ToProductSummary(item.Product),
            Variant = item.ProductVariant is null ? null : ResponseMapping.ToProductVariantSummary(item.ProductVariant),
            UnitPrice = unitPrice,
            TotalPrice = unitPrice * item.Quantity
        };
    }
}
