namespace WebApplication1.Contracts.Responses;

public class OrderResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int AddressId { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string? StripePaymentIntentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public UserSummaryResponse? User { get; set; }
    public AddressSummaryResponse? Address { get; set; }
    public List<OrderItemSummaryResponse> Items { get; set; } = new();
    public PaymentSummaryResponse? Payment { get; set; }
}
