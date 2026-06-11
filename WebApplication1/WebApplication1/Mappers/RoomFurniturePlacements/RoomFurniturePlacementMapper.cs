using Riok.Mapperly.Abstractions;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Mappers;

[Mapper]
public partial class RoomFurniturePlacementMapper : ICrudMapper<RoomFurniturePlacement, RoomFurniturePlacementRequest, RoomFurniturePlacementResponse>
{
    public partial RoomFurniturePlacement ToEntity(RoomFurniturePlacementRequest request);
    public partial void UpdateEntity([MappingTarget] RoomFurniturePlacement entity, RoomFurniturePlacementRequest request);
    public partial RoomFurniturePlacementResponse ToResponse(RoomFurniturePlacement entity);
}
