namespace WebApplication1.Exceptions;

public class InvalidCredentialsException(string message = "Invalid email or password.") : ApiException(message, StatusCodes.Status401Unauthorized);
