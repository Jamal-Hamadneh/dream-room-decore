namespace WebApplication1.Services;

public interface IOpenAiService
{
    Task<OpenAiRoomResult> AnalyzeRoomDesignAsync(string prompt, RoomAiPromptData data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Re-renders the deterministic composite (real product photos already placed at the user's
    /// chosen positions) into a single photorealistic interior photograph using gpt-image-1,
    /// preserving each product's exact appearance and placement. Returns null when no API key is
    /// configured or the call fails, so the caller can fall back to the composite.
    /// </summary>
    Task<byte[]?> GenerateRealisticRoomImageAsync(byte[] compositeImage, RoomAiPromptData data, CancellationToken cancellationToken = default);
}
