namespace WebApplication1.Services;

public class OpenAiRoomResult
{
    public string AnalysisJson { get; set; } = string.Empty;
    public string GeneratedImageSourceUrl { get; set; } = string.Empty;
    public string? GeneratedImageDataUri { get; set; }
}
