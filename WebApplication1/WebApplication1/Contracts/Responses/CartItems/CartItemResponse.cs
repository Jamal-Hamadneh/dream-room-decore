namespace WebApplication1.Contracts.Responses;

public class CartItemResponse
{
    public int Id { get; set; }
    public int CartId { get; set; }
    public int ProductId { get; set; }
    public int? ProductVariantId { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public ProductSummaryResponse? Product { get; set; }
    public ProductVariantSummaryResponse? Variant { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
