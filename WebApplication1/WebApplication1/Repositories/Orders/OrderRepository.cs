using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Repositories;

public interface IOrderRepository : ICrudRepository<Order>;

public class OrderRepository(ApplicationDbContext context) : CrudRepository<Order>(context), IOrderRepository;
