using Riok.Mapperly.Abstractions;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Mappers;

[Mapper]
public partial class CartItemMapper : ICrudMapper<CartItem, CartItemRequest, CartItemResponse>
{
    public partial CartItem ToEntity(CartItemRequest request);
    public partial void UpdateEntity([MappingTarget] CartItem entity, CartItemRequest request);
    public partial CartItemResponse ToResponse(CartItem entity);
}
