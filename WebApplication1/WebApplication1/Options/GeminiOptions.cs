namespace WebApplication1.Options;

public class GeminiOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-2.0-flash";
    public string ImageModel { get; set; } = "gemini-2.0-flash-preview-image-generation";
}
