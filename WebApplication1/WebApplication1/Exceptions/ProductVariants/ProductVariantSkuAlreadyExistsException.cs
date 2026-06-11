namespace WebApplication1.Exceptions.ProductVariants;

public class ProductVariantSkuAlreadyExistsException(string sku)
    : ConflictException($"Product variant with SKU '{sku}' already exists.");
