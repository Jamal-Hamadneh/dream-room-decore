namespace WebApplication1.Contracts.Requests;

public class RoomUploadRequest
{
    public int UserId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public decimal Height { get; set; }
    public decimal Width { get; set; }
    public decimal Depth { get; set; }
}
