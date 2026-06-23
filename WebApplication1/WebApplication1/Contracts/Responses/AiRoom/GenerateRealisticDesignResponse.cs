namespace WebApplication1.Contracts.Responses;

public class GenerateRealisticDesignResponse
{
    public int GeneratedRoomImageId { get; set; }
    public string GeneratedImageUrl { get; set; } = string.Empty;
    public string AiAnalysisJson { get; set; } = string.Empty;
}
