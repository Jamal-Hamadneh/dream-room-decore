namespace WebApplication1.Services;

public interface IOpenAiService
{
    Task<OpenAiRoomResult> GenerateRealisticRoomFromPreviewAsync(string prompt, RoomAiPromptData data, Stream previewImageStream, string previewImageContentType, string previewImageFileName, CancellationToken cancellationToken = default);
}
