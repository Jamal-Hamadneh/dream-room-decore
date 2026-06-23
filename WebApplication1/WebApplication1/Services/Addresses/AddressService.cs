using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Services;

public interface IAddressService : ICrudService<AddressRequest, AddressResponse>;

public class AddressService(IAddressRepository repository, ICrudMapper<Address, AddressRequest, AddressResponse> mapper)
    : CrudService<Address, AddressRequest, AddressResponse>(repository, mapper), IAddressService;
