using Microsoft.EntityFrameworkCore;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Data;
using WebApplication1.Exceptions.Reviews;
using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Services;

public interface IReviewService : ICrudService<ReviewRequest, ReviewResponse>;

public class ReviewService(IReviewRepository repository, ICrudMapper<Review, ReviewRequest, ReviewResponse> mapper, ApplicationDbContext context)
    : CrudService<Review, ReviewRequest, ReviewResponse>(repository, mapper), IReviewService
{
    public override async Task<List<ReviewResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var reviews = await Query().ToListAsync(cancellationToken);
        return reviews.Select(ToResponse).ToList();
    }

    public override async Task<ReviewResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var review = await Query().FirstOrDefaultAsync(review => review.Id == id, cancellationToken);
        return review is null ? null : ToResponse(review);
    }

    public override async Task<ReviewResponse> CreateAsync(ReviewRequest request, CancellationToken cancellationToken = default)
    {
        var exists = await context.Reviews.AnyAsync(review => review.UserId == request.UserId && review.ProductId == request.ProductId, cancellationToken);
        if (exists)
        {
            throw new UserAlreadyReviewedProductException(request.UserId, request.ProductId);
        }

        var response = await base.CreateAsync(request, cancellationToken);
        return await GetByIdAsync(response.Id, cancellationToken) ?? response;
    }

    private IQueryable<Review> Query() => context.Reviews
        .AsNoTracking()
        .Include(review => review.User)
        .Include(review => review.Product)
            .ThenInclude(product => product.ProductImages);

    private static ReviewResponse ToResponse(Review review) => new()
    {
        Id = review.Id,
        UserId = review.UserId,
        ProductId = review.ProductId,
        Rating = review.Rating,
        Comment = review.Comment,
        CreatedAt = review.CreatedAt,
        User = ResponseMapping.ToUserSummary(review.User),
        Product = ResponseMapping.ToProductSummary(review.Product)
    };
}
