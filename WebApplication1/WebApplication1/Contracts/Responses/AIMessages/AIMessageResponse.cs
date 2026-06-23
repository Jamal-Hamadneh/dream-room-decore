namespace WebApplication1.Contracts.Responses;

public class AIMessageResponse
{
    public int Id { get; set; }
    public int AIChatId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
