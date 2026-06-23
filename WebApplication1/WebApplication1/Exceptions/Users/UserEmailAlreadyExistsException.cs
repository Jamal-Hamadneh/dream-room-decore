namespace WebApplication1.Exceptions.Users;

public class UserEmailAlreadyExistsException(string email)
    : ConflictException($"User with email '{email}' already exists.");
