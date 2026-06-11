using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[ApiController]
[Authorize]
[Route("AiRoom")]
public class AiRoomController(IRoomAiService roomAiService) : ControllerBase
{
    [HttpPost("UploadAndCreateDesign")]
    public Task<UploadAndCreateDesignResponse> UploadAndCreateDesign([FromForm] UploadAndCreateDesignRequest request, CancellationToken cancellationToken)
    {
        return roomAiService.UploadAndCreateDesignAsync(GetCurrentUserId(), request, cancellationToken);
    }

    [HttpPost("SavePlacement")]
    public Task<PlacementResponse> SavePlacement(SavePlacementRequest request, CancellationToken cancellationToken)
    {
        return roomAiService.SavePlacementAsync(GetCurrentUserId(), request, cancellationToken);
    }

    [HttpPost("UpdatePlacement")]
    public Task<PlacementResponse> UpdatePlacement(UpdatePlacementRequest request, CancellationToken cancellationToken)
    {
        return roomAiService.UpdatePlacementAsync(GetCurrentUserId(), request, cancellationToken);
    }

    [HttpPost("SwitchProduct")]
    public Task<PlacementResponse> SwitchProduct(SwitchProductRequest request, CancellationToken cancellationToken)
    {
        return roomAiService.SwitchProductAsync(GetCurrentUserId(), request, cancellationToken);
    }

    [HttpPost("GenerateRealisticDesign")]
    public Task<GenerateRealisticDesignResponse> GenerateRealisticDesign(GenerateRealisticDesignRequest request, CancellationToken cancellationToken)
    {
        return roomAiService.GenerateRealisticDesignAsync(GetCurrentUserId(), request, cancellationToken);
    }

    private int GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userId, out var value) ? value : 0;
    }
}
