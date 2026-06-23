using Riok.Mapperly.Abstractions;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Mappers;

[Mapper]
public partial class ReviewMapper : ICrudMapper<Review, ReviewRequest, ReviewResponse>
{
    public partial Review ToEntity(ReviewRequest request);
    public partial void UpdateEntity([MappingTarget] Review entity, ReviewRequest request);
    public partial ReviewResponse ToResponse(Review entity);
}
