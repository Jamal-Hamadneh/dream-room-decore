namespace WebApplication1.Exceptions.AiRoom;

public class ProductMustBeInCartException(int productId)
    : ConflictException($"Product '{productId}' must be in your cart before it can be used in AI Room Design.");
