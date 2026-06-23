namespace WebApplication1.Contracts.Responses;

public class CreatePaymentIntentResponse
{
    public int OrderId { get; set; }
    public int PaymentId { get; set; }
    public string PaymentIntentId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string PublishableKey { get; set; } = string.Empty;
}
