namespace WebApplication1.Contracts.Responses;

public class ChatMessageDto
{
    public int Id { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<RecommendedProductResponse> RecommendedProducts { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}
