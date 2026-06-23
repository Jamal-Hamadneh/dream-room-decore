using Riok.Mapperly.Abstractions;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Mappers;

[Mapper]
public partial class ProductMapper : ICrudMapper<Product, ProductRequest, ProductResponse>
{
    public partial Product ToEntity(ProductRequest request);
    public partial void UpdateEntity([MappingTarget] Product entity, ProductRequest request);
    public partial ProductResponse ToResponse(Product entity);
}
