using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Repositories;

public interface IProductRepository : ICrudRepository<Product>;

public class ProductRepository(ApplicationDbContext context) : CrudRepository<Product>(context), IProductRepository;
