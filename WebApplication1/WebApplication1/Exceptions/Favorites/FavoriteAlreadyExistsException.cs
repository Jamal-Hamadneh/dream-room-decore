namespace WebApplication1.Exceptions.Favorites;

public class FavoriteAlreadyExistsException(int userId, int productId)
    : ConflictException($"User '{userId}' already has product '{productId}' in favorites.");
