namespace WebApplication1.Exceptions.Reviews;

public class UserAlreadyReviewedProductException(int userId, int productId)
    : ConflictException($"User '{userId}' already reviewed product '{productId}'.");
