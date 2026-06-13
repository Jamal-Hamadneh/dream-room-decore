using Microsoft.EntityFrameworkCore;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Services;

public interface IRoomDesignService : ICrudService<RoomDesignRequest, RoomDesignResponse>;

public class RoomDesignService(IRoomDesignRepository repository, ICrudMapper<RoomDesign, RoomDesignRequest, RoomDesignResponse> mapper, ApplicationDbContext context)
    : CrudService<RoomDesign, RoomDesignRequest, RoomDesignResponse>(repository, mapper), IRoomDesignService
{
    public override async Task<List<RoomDesignResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var designs = await Query().ToListAsync(cancellationToken);
        return designs.Select(ToResponse).ToList();
    }

    public override async Task<RoomDesignResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var design = await Query().FirstOrDefaultAsync(design => design.Id == id, cancellationToken);
        return design is null ? null : ToResponse(design);
    }

    public override async Task<RoomDesignResponse> CreateAsync(RoomDesignRequest request, CancellationToken cancellationToken = default)
    {
        var entity = Mapper.ToEntity(request);
        entity.CreatedAt = DateTime.UtcNow;
        var created = await Repository.AddAsync(entity, cancellationToken);

        var design = await Query().FirstAsync(d => d.Id == created.Id, cancellationToken);
        return ToResponse(design);
    }

    public override async Task<RoomDesignResponse?> UpdateAsync(int id, RoomDesignRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        Mapper.UpdateEntity(entity, request);
        entity.UpdatedAt = DateTime.UtcNow;
        await Repository.UpdateAsync(entity, cancellationToken);

        var design = await Query().FirstAsync(d => d.Id == id, cancellationToken);
        return ToResponse(design);
    }

    private IQueryable<RoomDesign> Query() => context.RoomDesigns
        .AsNoTracking()
        .Include(design => design.RoomUpload)
        .Include(design => design.GeneratedRoomImages)
        .Include(design => design.RoomFurniturePlacements)
            .ThenInclude(placement => placement.Product)
                .ThenInclude(product => product.ProductImages);

    private static RoomDesignResponse ToResponse(RoomDesign design) => new()
    {
        Id = design.Id,
        RoomUploadId = design.RoomUploadId,
        Name = design.Name,
        CreatedAt = design.CreatedAt,
        UpdatedAt = design.UpdatedAt,
        RoomUpload = new RoomUploadSummaryResponse
        {
            Id = design.RoomUpload.Id,
            ImageUrl = design.RoomUpload.ImageUrl,
            RoomType = design.RoomUpload.RoomType
        },
        Placements = design.RoomFurniturePlacements.Select(placement => new PlacementSummaryResponse
        {
            Id = placement.Id,
            Product = ResponseMapping.ToProductSummary(placement.Product),
            PositionX = placement.PositionX,
            PositionY = placement.PositionY,
            Rotation = placement.Rotation,
            Scale = placement.Scale,
            ZIndex = placement.ZIndex
        }).ToList(),
        GeneratedImageUrls = design.GeneratedRoomImages.Select(image => image.GeneratedImageUrl).ToList()
    };
}
