using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Repositories;

public interface IAddressRepository : ICrudRepository<Address>;

public class AddressRepository(ApplicationDbContext context) : CrudRepository<Address>(context), IAddressRepository;
