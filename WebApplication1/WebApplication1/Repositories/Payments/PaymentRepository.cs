using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Repositories;

public interface IPaymentRepository : ICrudRepository<Payment>;

public class PaymentRepository(ApplicationDbContext context) : CrudRepository<Payment>(context), IPaymentRepository;
