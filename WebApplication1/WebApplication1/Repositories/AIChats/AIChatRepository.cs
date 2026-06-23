using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Repositories;

public interface IAIChatRepository : ICrudRepository<AIChat>;

public class AIChatRepository(ApplicationDbContext context) : CrudRepository<AIChat>(context), IAIChatRepository;
