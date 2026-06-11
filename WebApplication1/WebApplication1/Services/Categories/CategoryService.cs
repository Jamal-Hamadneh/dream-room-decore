using Microsoft.EntityFrameworkCore;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Services;

public interface ICategoryService : ICrudService<CategoryRequest, CategoryResponse>;

public class CategoryService(ICategoryRepository repository, ICrudMapper<Category, CategoryRequest, CategoryResponse> mapper, ApplicationDbContext context)
    : CrudService<Category, CategoryRequest, CategoryResponse>(repository, mapper), ICategoryService
{
    public override async Task<List<CategoryResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var categories = await Query().ToListAsync(cancellationToken);
        return categories.Select(ToResponse).ToList();
    }

    public override async Task<CategoryResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await Query().FirstOrDefaultAsync(category => category.Id == id, cancellationToken);
        return category is null ? null : ToResponse(category);
    }

    private IQueryable<Category> Query() => context.Categories
        .AsNoTracking()
        .Include(category => category.Products)
            .ThenInclude(product => product.ProductImages);

    private static CategoryResponse ToResponse(Category category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        CreatedAt = category.CreatedAt,
        ProductsCount = category.Products.Count,
        Products = category.Products.Select(ResponseMapping.ToProductSummary).ToList()
    };
}
