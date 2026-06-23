using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Repositories;

public interface IProductImageRepository : ICrudRepository<ProductImage>;

public class ProductImageRepository(ApplicationDbContext context) : CrudRepository<ProductImage>(context), IProductImageRepository;
