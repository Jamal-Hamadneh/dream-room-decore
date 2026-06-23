using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Repositories;

public interface IReviewRepository : ICrudRepository<Review>;

public class ReviewRepository(ApplicationDbContext context) : CrudRepository<Review>(context), IReviewRepository;
