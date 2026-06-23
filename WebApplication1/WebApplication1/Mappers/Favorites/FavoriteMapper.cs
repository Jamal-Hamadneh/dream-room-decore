using Riok.Mapperly.Abstractions;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Mappers;

[Mapper]
public partial class FavoriteMapper : ICrudMapper<Favorite, FavoriteRequest, FavoriteResponse>
{
    public partial Favorite ToEntity(FavoriteRequest request);
    public partial void UpdateEntity([MappingTarget] Favorite entity, FavoriteRequest request);
    public partial FavoriteResponse ToResponse(Favorite entity);
}
