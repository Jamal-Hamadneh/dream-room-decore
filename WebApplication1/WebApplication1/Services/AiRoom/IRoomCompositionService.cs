namespace WebApplication1.Services;

public interface IRoomCompositionService
{
    Task<byte[]> ComposeRoomPreviewAsync(RoomAiPromptData data, CancellationToken cancellationToken = default);
}
