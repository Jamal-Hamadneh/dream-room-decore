using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Repositories;

public interface ICartItemRepository : ICrudRepository<CartItem>;

public class CartItemRepository(ApplicationDbContext context) : CrudRepository<CartItem>(context), ICartItemRepository;
