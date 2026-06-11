using Riok.Mapperly.Abstractions;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Mappers;

[Mapper]
public partial class ProductImageMapper : ICrudMapper<ProductImage, ProductImageRequest, ProductImageResponse>
{
    public partial ProductImage ToEntity(ProductImageRequest request);
    public partial void UpdateEntity([MappingTarget] ProductImage entity, ProductImageRequest request);
    public partial ProductImageResponse ToResponse(ProductImage entity);
}
