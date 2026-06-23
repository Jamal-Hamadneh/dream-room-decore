namespace WebApplication1.Exceptions;

public class ConflictException(string message) : ApiException(message, StatusCodes.Status409Conflict);
