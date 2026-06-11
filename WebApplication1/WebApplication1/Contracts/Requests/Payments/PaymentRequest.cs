namespace WebApplication1.Contracts.Requests;

public class PaymentRequest
{
    public int OrderId { get; set; }
    public string? StripePaymentIntentId { get; set; }
    public string? StripeChargeId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = "pending";
}
