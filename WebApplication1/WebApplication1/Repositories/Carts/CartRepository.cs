using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Repositories;

public interface ICartRepository : ICrudRepository<Cart>;

public class CartRepository(ApplicationDbContext context) : CrudRepository<Cart>(context), ICartRepository;
