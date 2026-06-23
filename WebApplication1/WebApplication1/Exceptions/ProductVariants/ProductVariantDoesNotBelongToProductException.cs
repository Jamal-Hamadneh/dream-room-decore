namespace WebApplication1.Exceptions.ProductVariants;

public class ProductVariantDoesNotBelongToProductException(int variantId, int productId)
    : ConflictException($"Product variant '{variantId}' does not belong to product '{productId}'.");
