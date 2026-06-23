namespace WebApplication1.Contracts.Responses;

public class SyncPaymentIntentResponse
{
    public int OrderId { get; set; }
    public int PaymentId { get; set; }
    public string PaymentIntentId { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string OrderStatus { get; set; } = string.Empty;
    public string StripeStatus { get; set; } = string.Empty;
}
