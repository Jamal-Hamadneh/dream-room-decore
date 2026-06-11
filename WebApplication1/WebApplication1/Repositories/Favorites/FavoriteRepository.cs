using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Repositories;

public interface IFavoriteRepository : ICrudRepository<Favorite>;

public class FavoriteRepository(ApplicationDbContext context) : CrudRepository<Favorite>(context), IFavoriteRepository;
