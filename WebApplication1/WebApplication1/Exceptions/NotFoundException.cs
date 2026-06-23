namespace WebApplication1.Exceptions;

public class NotFoundException(string message) : ApiException(message, StatusCodes.Status404NotFound);
