namespace WebApplication1.Contracts.Requests;

public class CartItemRequest
{
    public int CartId { get; set; }
    public int ProductId { get; set; }
    public int? ProductVariantId { get; set; }
    public int Quantity { get; set; }
}
