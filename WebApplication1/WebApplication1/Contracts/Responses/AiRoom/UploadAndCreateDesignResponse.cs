namespace WebApplication1.Contracts.Responses;

public class UploadAndCreateDesignResponse
{
    public int RoomUploadId { get; set; }
    public int RoomDesignId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public List<AiRoomProductResponse> CartProducts { get; set; } = new();
}
