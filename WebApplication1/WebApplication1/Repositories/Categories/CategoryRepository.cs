using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Repositories;

public interface ICategoryRepository : ICrudRepository<Category>;

public class CategoryRepository(ApplicationDbContext context) : CrudRepository<Category>(context), ICategoryRepository;
