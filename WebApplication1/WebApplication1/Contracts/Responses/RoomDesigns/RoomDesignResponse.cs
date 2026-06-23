namespace WebApplication1.Contracts.Responses;

public class RoomDesignResponse
{
    public int Id { get; set; }
    public int RoomUploadId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public RoomUploadSummaryResponse? RoomUpload { get; set; }
    public List<PlacementSummaryResponse> Placements { get; set; } = new();
    public List<string> GeneratedImageUrls { get; set; } = new();
}
