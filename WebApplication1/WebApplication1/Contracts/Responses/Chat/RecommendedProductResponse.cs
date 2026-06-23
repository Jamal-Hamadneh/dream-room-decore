namespace WebApplication1.Contracts.Responses;

public class RecommendedProductResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public string Category { get; set; } = string.Empty;
}
