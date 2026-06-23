using Riok.Mapperly.Abstractions;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Mappers;

[Mapper]
public partial class AddressMapper : ICrudMapper<Address, AddressRequest, AddressResponse>
{
    public partial Address ToEntity(AddressRequest request);
    public partial void UpdateEntity([MappingTarget] Address entity, AddressRequest request);
    public partial AddressResponse ToResponse(Address entity);
}
