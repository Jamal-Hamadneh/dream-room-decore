namespace WebApplication1.Exceptions.AiRoom;

public class RoomDesignAccessDeniedException(int roomDesignId)
    : ApiException($"You do not have access to room design '{roomDesignId}'.", StatusCodes.Status403Forbidden);
