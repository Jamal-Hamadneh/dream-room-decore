namespace WebApplication1.Exceptions.Products;

public class InsufficientProductStockException(int productId, int requestedQuantity, int availableQuantity)
    : ConflictException($"Product '{productId}' has only {availableQuantity} item(s) available, but {requestedQuantity} were requested.");
