using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Repositories;

public interface IRoomUploadRepository : ICrudRepository<RoomUpload>;

public class RoomUploadRepository(ApplicationDbContext context) : CrudRepository<RoomUpload>(context), IRoomUploadRepository;
