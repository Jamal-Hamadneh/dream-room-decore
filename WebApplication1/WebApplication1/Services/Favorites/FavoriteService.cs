using Microsoft.EntityFrameworkCore;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Data;
using WebApplication1.Exceptions.Favorites;
using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Services;

public interface IFavoriteService : ICrudService<FavoriteRequest, FavoriteResponse>;

public class FavoriteService(IFavoriteRepository repository, ICrudMapper<Favorite, FavoriteRequest, FavoriteResponse> mapper, ApplicationDbContext context)
    : CrudService<Favorite, FavoriteRequest, FavoriteResponse>(repository, mapper), IFavoriteService
{
    public override async Task<List<FavoriteResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var favorites = await Query().ToListAsync(cancellationToken);
        return favorites.Select(ToResponse).ToList();
    }

    public override async Task<FavoriteResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var favorite = await Query().FirstOrDefaultAsync(favorite => favorite.Id == id, cancellationToken);
        return favorite is null ? null : ToResponse(favorite);
    }

    public override async Task<FavoriteResponse> CreateAsync(FavoriteRequest request, CancellationToken cancellationToken = default)
    {
        var exists = await context.Favorites.AnyAsync(favorite => favorite.UserId == request.UserId && favorite.ProductId == request.ProductId, cancellationToken);
        if (exists)
        {
            throw new FavoriteAlreadyExistsException(request.UserId, request.ProductId);
        }

        var response = await base.CreateAsync(request, cancellationToken);
        return await GetByIdAsync(response.Id, cancellationToken) ?? response;
    }

    private IQueryable<Favorite> Query() => context.Favorites
        .AsNoTracking()
        .Include(favorite => favorite.Product)
            .ThenInclude(product => product.ProductImages);

    private static FavoriteResponse ToResponse(Favorite favorite) => new()
    {
        Id = favorite.Id,
        UserId = favorite.UserId,
        ProductId = favorite.ProductId,
        CreatedAt = favorite.CreatedAt,
        Product = ResponseMapping.ToProductSummary(favorite.Product)
    };
}
