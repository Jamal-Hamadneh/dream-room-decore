namespace WebApplication1.Services.OpenAI;

public class ChatAssistantReply
{
    public string Message { get; set; } = string.Empty;
    public List<int> RecommendedProductIds { get; set; } = new();
}
