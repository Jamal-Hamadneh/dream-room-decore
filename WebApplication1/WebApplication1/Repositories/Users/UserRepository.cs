using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Repositories;

public interface IUserRepository : ICrudRepository<User>;

public class UserRepository(ApplicationDbContext context) : CrudRepository<User>(context), IUserRepository;
