namespace WebApplication1.Contracts.Responses;

public class OrderItemResponse
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int? ProductVariantId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
    public ProductSummaryResponse? Product { get; set; }
    public ProductVariantSummaryResponse? Variant { get; set; }
}
