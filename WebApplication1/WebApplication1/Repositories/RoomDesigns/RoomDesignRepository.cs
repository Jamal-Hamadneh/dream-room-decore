using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Repositories;

public interface IRoomDesignRepository : ICrudRepository<RoomDesign>;

public class RoomDesignRepository(ApplicationDbContext context) : CrudRepository<RoomDesign>(context), IRoomDesignRepository;
