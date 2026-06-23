namespace WebApplication1.Exceptions.Carts;

public class UserCartAlreadyExistsException(int userId)
    : ConflictException($"User '{userId}' already has a cart.");
