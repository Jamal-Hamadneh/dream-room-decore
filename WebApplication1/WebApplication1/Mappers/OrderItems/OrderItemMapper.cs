using Riok.Mapperly.Abstractions;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Mappers;

[Mapper]
public partial class OrderItemMapper : ICrudMapper<OrderItem, OrderItemRequest, OrderItemResponse>
{
    public partial OrderItem ToEntity(OrderItemRequest request);
    public partial void UpdateEntity([MappingTarget] OrderItem entity, OrderItemRequest request);
    public partial OrderItemResponse ToResponse(OrderItem entity);
}
