using Riok.Mapperly.Abstractions;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Mappers;

[Mapper]
public partial class OrderMapper : ICrudMapper<Order, OrderRequest, OrderResponse>
{
    public partial Order ToEntity(OrderRequest request);
    public partial void UpdateEntity([MappingTarget] Order entity, OrderRequest request);
    public partial OrderResponse ToResponse(Order entity);
}
