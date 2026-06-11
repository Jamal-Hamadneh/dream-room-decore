using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;

namespace WebApplication1.Services;

public interface IRoomAiService
{
    Task<UploadAndCreateDesignResponse> UploadAndCreateDesignAsync(int userId, UploadAndCreateDesignRequest request, CancellationToken cancellationToken = default);
    Task<PlacementResponse> SavePlacementAsync(int userId, SavePlacementRequest request, CancellationToken cancellationToken = default);
    Task<PlacementResponse> UpdatePlacementAsync(int userId, UpdatePlacementRequest request, CancellationToken cancellationToken = default);
    Task<PlacementResponse> SwitchProductAsync(int userId, SwitchProductRequest request, CancellationToken cancellationToken = default);
    Task<GenerateRealisticDesignResponse> GenerateRealisticDesignAsync(int userId, GenerateRealisticDesignRequest request, CancellationToken cancellationToken = default);
}
