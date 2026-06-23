namespace WebApplication1.Exceptions.Auth;

public class InvalidRefreshTokenException(string message = "Invalid refresh token.")
    : ApiException(message, StatusCodes.Status401Unauthorized);
