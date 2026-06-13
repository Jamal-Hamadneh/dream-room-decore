namespace WebApplication1.Options;

public class OpenAiOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4o-mini";
    public string ImageModel { get; set; } = "gpt-image-1";
    public string ChatModel { get; set; } = "gpt-4o-mini";
}
