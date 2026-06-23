namespace WebApplication1.Exceptions.Payments;

public class StripePaymentException(string message) : ApiException(message, StatusCodes.Status400BadRequest);
