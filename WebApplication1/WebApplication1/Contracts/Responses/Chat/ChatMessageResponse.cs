namespace WebApplication1.Contracts.Responses;

public class ChatMessageResponse
{
    public int ConversationId { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<RecommendedProductResponse> RecommendedProducts { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}
