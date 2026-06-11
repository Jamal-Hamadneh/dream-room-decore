namespace WebApplication1.Contracts.Responses;

public class ProductResponse
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int StockQuantity { get; set; }
    public string? Material { get; set; }
    public string? Color { get; set; }
    public decimal? Height { get; set; }
    public decimal? Width { get; set; }
    public decimal? Depth { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewsCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public CategorySummaryResponse? Category { get; set; }
    public string? MainImageUrl { get; set; }
    public List<ProductImageSummaryResponse> Images { get; set; } = new();
    public List<ProductVariantSummaryResponse> Variants { get; set; } = new();
}
