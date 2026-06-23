using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Repositories;

public interface IOrderItemRepository : ICrudRepository<OrderItem>;

public class OrderItemRepository(ApplicationDbContext context) : CrudRepository<OrderItem>(context), IOrderItemRepository;
