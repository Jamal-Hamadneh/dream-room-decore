namespace WebApplication1.Contracts.Responses;

public class CategoryResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int ProductsCount { get; set; }
    public List<ProductSummaryResponse> Products { get; set; } = new();
}
