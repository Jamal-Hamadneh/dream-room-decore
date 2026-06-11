namespace WebApplication1.Services;

public interface IOpenAiService
{
    Task<OpenAiRoomResult> GenerateRealisticRoomAsync(string prompt, RoomAiPromptData data, CancellationToken cancellationToken = default);
}
