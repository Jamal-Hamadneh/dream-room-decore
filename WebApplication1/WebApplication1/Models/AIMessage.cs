namespace WebApplication1.Models;

public class AIMessage
{
    public int Id { get; set; }
    public int AIChatId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public AIChat AIChat { get; set; } = null!;
}
