namespace WebApplication1.Exceptions;

public class RequestValidationException(IDictionary<string, string[]> errors) : ApiException("Validation failed.", StatusCodes.Status400BadRequest)
{
    public IDictionary<string, string[]> Errors { get; } = errors;
}
