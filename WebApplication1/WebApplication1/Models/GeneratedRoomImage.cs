namespace WebApplication1.Models;

public class GeneratedRoomImage
{
    public int Id { get; set; }
    public int RoomDesignId { get; set; }
    public string GeneratedImageUrl { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public string AiAnalysisJson { get; set; } = string.Empty;
    public string Status { get; set; } = "completed";
    public DateTime CreatedAt { get; set; }

    public RoomDesign RoomDesign { get; set; } = null!;
}
