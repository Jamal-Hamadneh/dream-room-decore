namespace WebApplication1.Contracts.Responses;

public class StripeConfigResponse
{
    public string PublishableKey { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public bool IsConfigured { get; set; }
}
