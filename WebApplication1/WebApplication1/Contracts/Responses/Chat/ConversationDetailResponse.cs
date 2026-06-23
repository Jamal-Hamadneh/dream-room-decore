namespace WebApplication1.Contracts.Responses;

public class ConversationDetailResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<ChatMessageDto> Messages { get; set; } = [];
}
