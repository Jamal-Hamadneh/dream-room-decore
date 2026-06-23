namespace WebApplication1.Services;

public interface IRoomCompositionService
{
    Task<byte[]> ComposeRoomImageAsync(RoomAiPromptData data, CancellationToken cancellationToken = default);
}
