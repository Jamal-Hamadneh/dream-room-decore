namespace WebApplication1.Contracts.Requests;

public class SendChatMessageRequest
{
    public int? ConversationId { get; set; }
    public string Message { get; set; } = string.Empty;
}
