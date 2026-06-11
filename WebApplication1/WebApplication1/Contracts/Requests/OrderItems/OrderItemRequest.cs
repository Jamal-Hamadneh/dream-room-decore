namespace WebApplication1.Contracts.Requests;

public class OrderItemRequest
{
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int? ProductVariantId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
