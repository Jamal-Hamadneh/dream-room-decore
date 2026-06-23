namespace WebApplication1.Contracts.Responses;

public class UserSummaryResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class CategorySummaryResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ProductSummaryResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public string? MainImageUrl { get; set; }
    public bool IsActive { get; set; }
}

public class ProductImageSummaryResponse
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsMain { get; set; }
}

public class ProductVariantSummaryResponse
{
    public int Id { get; set; }
    public string? Color { get; set; }
    public string? Size { get; set; }
    public string? Material { get; set; }
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}

public class AddressSummaryResponse
{
    public int Id { get; set; }
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string? Building { get; set; }
    public bool IsDefault { get; set; }
}

public class CartItemSummaryResponse
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int? ProductVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public string? VariantSku { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class OrderItemSummaryResponse
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int? ProductVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public class PaymentSummaryResponse
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class RoomUploadSummaryResponse
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
}

public class PlacementSummaryResponse
{
    public int Id { get; set; }
    public ProductSummaryResponse Product { get; set; } = new();
    public decimal PositionX { get; set; }
    public decimal PositionY { get; set; }
    public decimal Rotation { get; set; }
    public decimal Scale { get; set; }
    public int ZIndex { get; set; }
}
