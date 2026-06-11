namespace WebApplication1.Contracts.Requests;

public class OrderRequest
{
    public int UserId { get; set; }
    public int AddressId { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = "pending";
    public string PaymentStatus { get; set; } = "pending";
    public string? StripePaymentIntentId { get; set; }
}
