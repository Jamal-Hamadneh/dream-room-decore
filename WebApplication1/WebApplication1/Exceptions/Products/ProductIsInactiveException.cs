namespace WebApplication1.Exceptions.Products;

public class ProductIsInactiveException(int productId)
    : ConflictException($"Product '{productId}' is inactive.");
