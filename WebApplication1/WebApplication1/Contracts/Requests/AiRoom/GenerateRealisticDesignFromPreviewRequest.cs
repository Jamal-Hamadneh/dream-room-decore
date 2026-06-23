using Microsoft.AspNetCore.Http;

namespace WebApplication1.Contracts.Requests;

public class GenerateRealisticDesignFromPreviewRequest
{
    public int RoomDesignId { get; set; }
    public IFormFile PreviewImage { get; set; } = null!;
}
