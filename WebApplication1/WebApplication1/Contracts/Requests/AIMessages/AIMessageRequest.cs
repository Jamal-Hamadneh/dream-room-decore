namespace WebApplication1.Contracts.Requests;

public class AIMessageRequest
{
    public int AIChatId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
