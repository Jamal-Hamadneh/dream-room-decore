namespace WebApplication1.Contracts.Requests;

public class ProductVariantRequest
{
    public int ProductId { get; set; }
    public string? Color { get; set; }
    public string? Size { get; set; }
    public string? Material { get; set; }
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}
