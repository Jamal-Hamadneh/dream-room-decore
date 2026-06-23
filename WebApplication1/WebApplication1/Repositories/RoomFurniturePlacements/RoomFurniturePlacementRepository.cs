using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Repositories;

public interface IRoomFurniturePlacementRepository : ICrudRepository<RoomFurniturePlacement>;

public class RoomFurniturePlacementRepository(ApplicationDbContext context) : CrudRepository<RoomFurniturePlacement>(context), IRoomFurniturePlacementRepository;
