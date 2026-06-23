using Riok.Mapperly.Abstractions;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Mappers;

[Mapper]
public partial class CategoryMapper : ICrudMapper<Category, CategoryRequest, CategoryResponse>
{
    public partial Category ToEntity(CategoryRequest request);
    public partial void UpdateEntity([MappingTarget] Category entity, CategoryRequest request);
    public partial CategoryResponse ToResponse(Category entity);
}
