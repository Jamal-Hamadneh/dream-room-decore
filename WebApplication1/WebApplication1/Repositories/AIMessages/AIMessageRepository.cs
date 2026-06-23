using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Repositories;

public interface IAIMessageRepository : ICrudRepository<AIMessage>;

public class AIMessageRepository(ApplicationDbContext context) : CrudRepository<AIMessage>(context), IAIMessageRepository;
