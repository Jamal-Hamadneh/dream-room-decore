using Microsoft.EntityFrameworkCore;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Data;
using WebApplication1.Exceptions.ProductVariants;
using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Services;

public interface IProductVariantService : ICrudService<ProductVariantRequest, ProductVariantResponse>;

public class ProductVariantService(IProductVariantRepository repository, ICrudMapper<ProductVariant, ProductVariantRequest, ProductVariantResponse> mapper, ApplicationDbContext context)
    : CrudService<ProductVariant, ProductVariantRequest, ProductVariantResponse>(repository, mapper), IProductVariantService
{
    public override async Task<List<ProductVariantResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var variants = await Query().ToListAsync(cancellationToken);
        return variants.Select(ToResponse).ToList();
    }

    public override async Task<ProductVariantResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var variant = await Query().FirstOrDefaultAsync(variant => variant.Id == id, cancellationToken);
        return variant is null ? null : ToResponse(variant);
    }

    public override async Task<ProductVariantResponse> CreateAsync(ProductVariantRequest request, CancellationToken cancellationToken = default)
    {
        await ThrowIfSkuExistsAsync(request.Sku, null, cancellationToken);
        return await base.CreateAsync(request, cancellationToken);
    }

    public override async Task<ProductVariantResponse?> UpdateAsync(int id, ProductVariantRequest request, CancellationToken cancellationToken = default)
    {
        await ThrowIfSkuExistsAsync(request.Sku, id, cancellationToken);
        return await base.UpdateAsync(id, request, cancellationToken);
    }

    private async Task ThrowIfSkuExistsAsync(string sku, int? currentVariantId, CancellationToken cancellationToken)
    {
        var normalizedSku = sku.Trim();
        var exists = await context.ProductVariants.AnyAsync(variant => variant.Sku == normalizedSku && variant.Id != currentVariantId, cancellationToken);
        if (exists)
        {
            throw new ProductVariantSkuAlreadyExistsException(normalizedSku);
        }
    }

    private IQueryable<ProductVariant> Query() => context.ProductVariants
        .AsNoTracking()
        .Include(variant => variant.Product)
            .ThenInclude(product => product.ProductImages);

    private static ProductVariantResponse ToResponse(ProductVariant variant) => new()
    {
        Id = variant.Id,
        ProductId = variant.ProductId,
        Color = variant.Color,
        Size = variant.Size,
        Material = variant.Material,
        Sku = variant.Sku,
        Price = variant.Price,
        StockQuantity = variant.StockQuantity,
        CreatedAt = variant.CreatedAt,
        Product = ResponseMapping.ToProductSummary(variant.Product)
    };
}
