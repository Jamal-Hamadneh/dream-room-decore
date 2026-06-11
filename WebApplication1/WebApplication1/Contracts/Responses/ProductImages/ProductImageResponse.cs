namespace WebApplication1.Contracts.Responses;

public class ProductImageResponse
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsMain { get; set; }
    public DateTime CreatedAt { get; set; }
    public ProductSummaryResponse? Product { get; set; }
}
