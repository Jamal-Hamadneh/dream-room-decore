namespace WebApplication1.Contracts.Responses;

public class TawkConfigResponse
{
    public string PropertyId { get; set; } = string.Empty;
    public string WidgetId { get; set; } = string.Empty;
    public string EmbedUrl { get; set; } = string.Empty;
    public bool IsConfigured { get; set; }
}
