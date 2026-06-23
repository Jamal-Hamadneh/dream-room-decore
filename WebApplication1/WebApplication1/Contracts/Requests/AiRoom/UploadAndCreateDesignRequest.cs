using Microsoft.AspNetCore.Http;

namespace WebApplication1.Contracts.Requests;

public class UploadAndCreateDesignRequest
{
    public IFormFile RoomImage { get; set; } = null!;
    public string? RoomType { get; set; }
    public decimal? Height { get; set; }
    public decimal? Width { get; set; }
    public decimal? Depth { get; set; }
}
