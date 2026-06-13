using Microsoft.EntityFrameworkCore;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Services;

public interface IProductService : ICrudService<ProductRequest, ProductResponse>
{
    Task<List<TopProductResponse>> GetTopProductsAsync(int limit, CancellationToken cancellationToken = default);
}

public class ProductService(IProductRepository repository, ICrudMapper<Product, ProductRequest, ProductResponse> mapper, ApplicationDbContext context)
    : CrudService<Product, ProductRequest, ProductResponse>(repository, mapper), IProductService
{
    public override async Task<List<ProductResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = await Query().ToListAsync(cancellationToken);
        return products.Select(ToResponse).ToList();
    }

    public override async Task<ProductResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await Query().FirstOrDefaultAsync(product => product.Id == id, cancellationToken);
        return product is null ? null : ToResponse(product);
    }

    public async Task<List<TopProductResponse>> GetTopProductsAsync(int limit, CancellationToken cancellationToken = default)
    {
        limit = limit < 1 ? 4 : limit;

        var topSales = await context.OrderItems
            .AsNoTracking()
            .Where(item => item.Order.PaymentStatus == "paid")
            .GroupBy(item => item.ProductId)
            .Select(group => new { ProductId = group.Key, SalesCount = group.Sum(item => item.Quantity) })
            .OrderByDescending(group => group.SalesCount)
            .Take(limit)
            .ToListAsync(cancellationToken);

        var productIds = topSales.Select(sales => sales.ProductId).ToList();
        var products = await context.Products
            .AsNoTracking()
            .Include(product => product.ProductImages)
            .Where(product => productIds.Contains(product.Id))
            .ToListAsync(cancellationToken);

        return topSales
            .Select(sales =>
            {
                var product = products.First(p => p.Id == sales.ProductId);
                return new TopProductResponse
                {
                    Id = product.Id,
                    Name = product.Name,
                    ImageUrl = product.ProductImages.OrderByDescending(image => image.IsMain).FirstOrDefault()?.ImageUrl,
                    SalesCount = sales.SalesCount,
                    AverageRating = product.AverageRating,
                    ReviewsCount = product.ReviewsCount,
                    Price = product.Price
                };
            })
            .ToList();
    }

    private IQueryable<Product> Query() => context.Products
        .AsNoTracking()
        .Include(product => product.Category)
        .Include(product => product.ProductImages)
        .Include(product => product.ProductVariants);

    private static ProductResponse ToResponse(Product product) => new()
    {
        Id = product.Id,
        CategoryId = product.CategoryId,
        Name = product.Name,
        Description = product.Description,
        Price = product.Price,
        DiscountPrice = product.DiscountPrice,
        StockQuantity = product.StockQuantity,
        Material = product.Material,
        Color = product.Color,
        Height = product.Height,
        Width = product.Width,
        Depth = product.Depth,
        IsActive = product.IsActive,
        IsFeatured = product.IsFeatured,
        AverageRating = product.AverageRating,
        ReviewsCount = product.ReviewsCount,
        CreatedAt = product.CreatedAt,
        UpdatedAt = product.UpdatedAt,
        Category = ResponseMapping.ToCategorySummary(product.Category),
        MainImageUrl = product.ProductImages.OrderByDescending(image => image.IsMain).FirstOrDefault()?.ImageUrl,
        Images = product.ProductImages.Select(ResponseMapping.ToProductImageSummary).ToList(),
        Variants = product.ProductVariants.Select(ResponseMapping.ToProductVariantSummary).ToList()
    };
}
