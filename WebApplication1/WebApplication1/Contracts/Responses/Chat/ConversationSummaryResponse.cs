namespace WebApplication1.Contracts.Responses;

public class ConversationSummaryResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? LastMessagePreview { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
