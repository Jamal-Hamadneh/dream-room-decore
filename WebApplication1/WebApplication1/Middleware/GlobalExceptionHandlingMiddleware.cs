using Microsoft.EntityFrameworkCore;
using WebApplication1.Exceptions;

namespace WebApplication1.Middleware;

public class GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = exception switch
        {
            ApiException apiException => apiException.StatusCode,
            DbUpdateException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception occurred.");
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        object response = exception switch
        {
            RequestValidationException validationException => new
            {
                title = validationException.Message,
                status = statusCode,
                errors = validationException.Errors
            },
            DbUpdateException => new
            {
                title = "Database operation failed.",
                status = statusCode,
                detail = "Check related ids, unique fields, and required values."
            },
            ApiException => new
            {
                title = exception.Message,
                status = statusCode
            },
            _ => new
            {
                title = "An unexpected error occurred.",
                status = statusCode
            }
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}
