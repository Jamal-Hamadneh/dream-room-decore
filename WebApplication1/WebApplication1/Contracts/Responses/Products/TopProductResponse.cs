namespace WebApplication1.Contracts.Responses;

public class TopProductResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int SalesCount { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewsCount { get; set; }
    public decimal Price { get; set; }
}
