namespace WebApplication1.Contracts.Requests;

public class AIChatRequest
{
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
}
