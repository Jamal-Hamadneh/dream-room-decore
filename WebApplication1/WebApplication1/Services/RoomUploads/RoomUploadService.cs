using Microsoft.EntityFrameworkCore;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Services;

public interface IRoomUploadService : ICrudService<RoomUploadRequest, RoomUploadResponse>;

public class RoomUploadService(IRoomUploadRepository repository, ICrudMapper<RoomUpload, RoomUploadRequest, RoomUploadResponse> mapper, ApplicationDbContext context)
    : CrudService<RoomUpload, RoomUploadRequest, RoomUploadResponse>(repository, mapper), IRoomUploadService
{
    public override async Task<List<RoomUploadResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var uploads = await Query().ToListAsync(cancellationToken);
        return uploads.Select(ToResponse).ToList();
    }

    public override async Task<RoomUploadResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var upload = await Query().FirstOrDefaultAsync(upload => upload.Id == id, cancellationToken);
        return upload is null ? null : ToResponse(upload);
    }

    private IQueryable<RoomUpload> Query() => context.RoomUploads
        .AsNoTracking()
        .Include(upload => upload.RoomDesigns);

    private static RoomUploadResponse ToResponse(RoomUpload upload) => new()
    {
        Id = upload.Id,
        UserId = upload.UserId,
        ImageUrl = upload.ImageUrl,
        RoomType = upload.RoomType,
        Height = upload.Height,
        Width = upload.Width,
        Depth = upload.Depth,
        AiDetectedWidth = upload.AiDetectedWidth,
        AiDetectedHeight = upload.AiDetectedHeight,
        AiDetectedDepth = upload.AiDetectedDepth,
        AiDescription = upload.AiDescription,
        CreatedAt = upload.CreatedAt,
        Designs = upload.RoomDesigns.Select(design => new RoomDesignSummaryResponse
        {
            Id = design.Id,
            Name = design.Name,
            CreatedAt = design.CreatedAt
        }).ToList()
    };
}
