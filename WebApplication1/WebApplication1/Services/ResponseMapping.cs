using WebApplication1.Contracts.Responses;
using WebApplication1.Models;

namespace WebApplication1.Services;

public static class ResponseMapping
{
    public static UserSummaryResponse ToUserSummary(User user) => new()
    {
        Id = user.Id,
        FullName = $"{user.FirstName} {user.LastName}".Trim(),
        Email = user.Email
    };

    public static CategorySummaryResponse ToCategorySummary(Category category) => new()
    {
        Id = category.Id,
        Name = category.Name
    };

    public static ProductSummaryResponse ToProductSummary(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Price = product.Price,
        DiscountPrice = product.DiscountPrice,
        IsActive = product.IsActive,
        MainImageUrl = product.ProductImages.OrderByDescending(image => image.IsMain).FirstOrDefault()?.ImageUrl
    };

    public static ProductImageSummaryResponse ToProductImageSummary(ProductImage image) => new()
    {
        Id = image.Id,
        ImageUrl = image.ImageUrl,
        IsMain = image.IsMain
    };

    public static ProductVariantSummaryResponse ToProductVariantSummary(ProductVariant variant) => new()
    {
        Id = variant.Id,
        Color = variant.Color,
        Size = variant.Size,
        Material = variant.Material,
        Sku = variant.Sku,
        Price = variant.Price,
        StockQuantity = variant.StockQuantity
    };

    public static AddressSummaryResponse ToAddressSummary(Address address) => new()
    {
        Id = address.Id,
        Country = address.Country,
        City = address.City,
        Street = address.Street,
        Building = address.Building,
        IsDefault = address.IsDefault
    };

    public static CartItemSummaryResponse ToCartItemSummary(CartItem item)
    {
        var unitPrice = item.ProductVariant?.Price ?? item.Product.DiscountPrice ?? item.Product.Price;

        return new CartItemSummaryResponse
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductVariantId = item.ProductVariantId,
            ProductName = item.Product.Name,
            ProductImageUrl = item.Product.ProductImages.OrderByDescending(image => image.IsMain).FirstOrDefault()?.ImageUrl,
            VariantSku = item.ProductVariant?.Sku,
            Quantity = item.Quantity,
            UnitPrice = unitPrice,
            TotalPrice = unitPrice * item.Quantity
        };
    }

    public static OrderItemSummaryResponse ToOrderItemSummary(OrderItem item) => new()
    {
        Id = item.Id,
        ProductId = item.ProductId,
        ProductVariantId = item.ProductVariantId,
        ProductName = item.Product.Name,
        ProductImageUrl = item.Product.ProductImages.OrderByDescending(image => image.IsMain).FirstOrDefault()?.ImageUrl,
        Quantity = item.Quantity,
        Price = item.Price
    };

    public static PaymentSummaryResponse ToPaymentSummary(Payment payment) => new()
    {
        Id = payment.Id,
        Amount = payment.Amount,
        Currency = payment.Currency,
        Status = payment.Status,
        CreatedAt = payment.CreatedAt
    };
}
