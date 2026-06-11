namespace WebApplication1.Contracts.Responses;

public class CartResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public UserSummaryResponse? User { get; set; }
    public List<CartItemSummaryResponse> Items { get; set; } = new();
    public int ItemsCount { get; set; }
    public decimal TotalPrice { get; set; }
}
