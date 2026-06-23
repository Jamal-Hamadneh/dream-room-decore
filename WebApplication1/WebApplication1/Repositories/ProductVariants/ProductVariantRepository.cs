using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Repositories;

public interface IProductVariantRepository : ICrudRepository<ProductVariant>;

public class ProductVariantRepository(ApplicationDbContext context) : CrudRepository<ProductVariant>(context), IProductVariantRepository;
