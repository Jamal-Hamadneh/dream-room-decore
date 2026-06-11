using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Contracts.Responses;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/chatbot")]
public class ChatbotController(IChatbotContextService chatbotContextService) : ControllerBase
{
    [HttpGet("config")]
    [AllowAnonymous]
    public ActionResult<ChatwootConfigResponse> GetConfig()
    {
        return Ok(chatbotContextService.GetChatwootConfig());
    }

    [HttpGet("context")]
    [Authorize]
    public Task<ChatbotContextResponse> GetContext(CancellationToken cancellationToken)
    {
        return chatbotContextService.GetContextAsync(GetCurrentUserId(), cancellationToken);
    }

    private int GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userId, out var value) ? value : 0;
    }
}
