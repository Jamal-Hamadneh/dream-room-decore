namespace WebApplication1.Models;

public class Payment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string? StripePaymentIntentId { get; set; }
    public string? StripeChargeId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; }

    public Order Order { get; set; } = null!;
}
