using Riok.Mapperly.Abstractions;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Mappers;

[Mapper]
public partial class CartMapper : ICrudMapper<Cart, CartRequest, CartResponse>
{
    public partial Cart ToEntity(CartRequest request);
    public partial void UpdateEntity([MappingTarget] Cart entity, CartRequest request);
    public partial CartResponse ToResponse(Cart entity);
}
