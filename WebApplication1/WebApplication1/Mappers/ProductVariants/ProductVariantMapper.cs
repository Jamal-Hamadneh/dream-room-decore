using Riok.Mapperly.Abstractions;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Mappers;

[Mapper]
public partial class ProductVariantMapper : ICrudMapper<ProductVariant, ProductVariantRequest, ProductVariantResponse>
{
    public partial ProductVariant ToEntity(ProductVariantRequest request);
    public partial void UpdateEntity([MappingTarget] ProductVariant entity, ProductVariantRequest request);
    public partial ProductVariantResponse ToResponse(ProductVariant entity);
}
