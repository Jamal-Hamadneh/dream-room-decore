using Microsoft.EntityFrameworkCore;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Services;

public interface IProductImageService : ICrudService<ProductImageRequest, ProductImageResponse>;

public class ProductImageService(IProductImageRepository repository, ICrudMapper<ProductImage, ProductImageRequest, ProductImageResponse> mapper, ApplicationDbContext context)
    : CrudService<ProductImage, ProductImageRequest, ProductImageResponse>(repository, mapper), IProductImageService
{
    public override async Task<List<ProductImageResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var images = await Query().ToListAsync(cancellationToken);
        var mainImageUrls = await GetMainImageUrlsAsync(images.Select(image => image.ProductId), cancellationToken);
        return images.Select(image => ToResponse(image, mainImageUrls)).ToList();
    }

    public override async Task<ProductImageResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var image = await Query().FirstOrDefaultAsync(image => image.Id == id, cancellationToken);
        if (image is null)
        {
            return null;
        }

        var mainImageUrls = await GetMainImageUrlsAsync([image.ProductId], cancellationToken);
        return ToResponse(image, mainImageUrls);
    }

    public override async Task<ProductImageResponse> CreateAsync(ProductImageRequest request, CancellationToken cancellationToken = default)
    {
        var image = new ProductImage
        {
            ProductId = request.ProductId,
            ImageUrl = request.ImageUrl,
            IsMain = request.IsMain,
            CreatedAt = DateTime.UtcNow
        };

        context.ProductImages.Add(image);
        await context.SaveChangesAsync(cancellationToken);

        return (await GetByIdAsync(image.Id, cancellationToken))!;
    }

    public override async Task<ProductImageResponse?> UpdateAsync(int id, ProductImageRequest request, CancellationToken cancellationToken = default)
    {
        var image = await context.ProductImages.FindAsync([id], cancellationToken);
        if (image is null)
        {
            return null;
        }

        image.ProductId = request.ProductId;
        image.ImageUrl = request.ImageUrl;
        image.IsMain = request.IsMain;
        await context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    private IQueryable<ProductImage> Query() => context.ProductImages
        .AsNoTracking()
        .Include(image => image.Product);

    private async Task<Dictionary<int, string?>> GetMainImageUrlsAsync(IEnumerable<int> productIds, CancellationToken cancellationToken)
    {
        var ids = productIds.Distinct().ToList();

        return await context.Products
            .AsNoTracking()
            .Where(product => ids.Contains(product.Id))
            .Select(product => new
            {
                product.Id,
                MainImageUrl = product.ProductImages
                    .OrderByDescending(image => image.IsMain)
                    .Select(image => image.ImageUrl)
                    .FirstOrDefault()
            })
            .ToDictionaryAsync(product => product.Id, product => product.MainImageUrl, cancellationToken);
    }

    private static ProductImageResponse ToResponse(ProductImage image, IReadOnlyDictionary<int, string?> mainImageUrls) => new()
    {
        Id = image.Id,
        ProductId = image.ProductId,
        ImageUrl = image.ImageUrl,
        IsMain = image.IsMain,
        CreatedAt = image.CreatedAt,
        Product = new ProductSummaryResponse
        {
            Id = image.Product.Id,
            Name = image.Product.Name,
            Price = image.Product.Price,
            DiscountPrice = image.Product.DiscountPrice,
            MainImageUrl = mainImageUrls.GetValueOrDefault(image.ProductId),
            IsActive = image.Product.IsActive
        }
    };
}
