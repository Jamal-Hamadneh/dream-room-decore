using Riok.Mapperly.Abstractions;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Mappers;

[Mapper]
public partial class RoomUploadMapper : ICrudMapper<RoomUpload, RoomUploadRequest, RoomUploadResponse>
{
    public partial RoomUpload ToEntity(RoomUploadRequest request);
    public partial void UpdateEntity([MappingTarget] RoomUpload entity, RoomUploadRequest request);
    public partial RoomUploadResponse ToResponse(RoomUpload entity);
}
