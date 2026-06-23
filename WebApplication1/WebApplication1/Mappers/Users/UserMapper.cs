using Riok.Mapperly.Abstractions;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Mappers;

[Mapper]
public partial class UserMapper : ICrudMapper<User, UserRequest, UserResponse>
{
    public partial User ToEntity(UserRequest request);
    public partial void UpdateEntity([MappingTarget] User entity, UserRequest request);
    public partial UserResponse ToResponse(User entity);
}
