using Microsoft.EntityFrameworkCore;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Services;

public interface IRoomFurniturePlacementService : ICrudService<RoomFurniturePlacementRequest, RoomFurniturePlacementResponse>;

public class RoomFurniturePlacementService(IRoomFurniturePlacementRepository repository, ICrudMapper<RoomFurniturePlacement, RoomFurniturePlacementRequest, RoomFurniturePlacementResponse> mapper, ApplicationDbContext context)
    : CrudService<RoomFurniturePlacement, RoomFurniturePlacementRequest, RoomFurniturePlacementResponse>(repository, mapper), IRoomFurniturePlacementService
{
    public override async Task<List<RoomFurniturePlacementResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var placements = await Query().ToListAsync(cancellationToken);
        return placements.Select(ToResponse).ToList();
    }

    public override async Task<RoomFurniturePlacementResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var placement = await Query().FirstOrDefaultAsync(placement => placement.Id == id, cancellationToken);
        return placement is null ? null : ToResponse(placement);
    }

    public override async Task<RoomFurniturePlacementResponse> CreateAsync(RoomFurniturePlacementRequest request, CancellationToken cancellationToken = default)
    {
        var response = await base.CreateAsync(request, cancellationToken);
        return await GetByIdAsync(response.Id, cancellationToken) ?? response;
    }

    public override async Task<RoomFurniturePlacementResponse?> UpdateAsync(int id, RoomFurniturePlacementRequest request, CancellationToken cancellationToken = default)
    {
        var response = await base.UpdateAsync(id, request, cancellationToken);
        return response is null ? null : await GetByIdAsync(id, cancellationToken);
    }

    private IQueryable<RoomFurniturePlacement> Query() => context.RoomFurniturePlacements
        .AsNoTracking()
        .Include(placement => placement.Product)
            .ThenInclude(product => product.ProductImages);

    private static RoomFurniturePlacementResponse ToResponse(RoomFurniturePlacement placement) => new()
    {
        Id = placement.Id,
        RoomDesignId = placement.RoomDesignId,
        ProductId = placement.ProductId,
        PositionX = placement.PositionX,
        PositionY = placement.PositionY,
        Rotation = placement.Rotation,
        Scale = placement.Scale,
        ZIndex = placement.ZIndex,
        CreatedAt = placement.CreatedAt,
        UpdatedAt = placement.UpdatedAt,
        Product = ResponseMapping.ToProductSummary(placement.Product)
    };
}
