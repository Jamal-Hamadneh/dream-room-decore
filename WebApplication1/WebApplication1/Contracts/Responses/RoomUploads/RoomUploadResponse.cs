namespace WebApplication1.Contracts.Responses;

public class RoomUploadResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public decimal Height { get; set; }
    public decimal Width { get; set; }
    public decimal Depth { get; set; }
    public decimal? AiDetectedWidth { get; set; }
    public decimal? AiDetectedHeight { get; set; }
    public decimal? AiDetectedDepth { get; set; }
    public string? AiDescription { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<RoomDesignSummaryResponse> Designs { get; set; } = new();
}
