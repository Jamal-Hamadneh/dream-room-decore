using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Exceptions;
using WebApplication1.Services.Chat;

namespace WebApplication1.Controllers;

[ApiController]
[Authorize]
[Route("api/chat")]
public class ChatController(
    IChatAssistantService chatAssistantService,
    IValidator<SendChatMessageRequest> sendMessageValidator,
    IValidator<CreateConversationRequest> createConversationValidator) : ControllerBase
{
    [HttpPost("message")]
    [EnableRateLimiting("chat")]
    public async Task<ActionResult<ChatMessageResponse>> SendMessage(SendChatMessageRequest request, CancellationToken cancellationToken)
    {
        var validation = await sendMessageValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new RequestValidationException(validation.ToDictionary());
        }

        return Ok(await chatAssistantService.SendMessageAsync(GetCurrentUserId(), request, cancellationToken));
    }

    [HttpGet("conversations")]
    public async Task<ActionResult<List<ConversationSummaryResponse>>> GetConversations(CancellationToken cancellationToken)
    {
        return Ok(await chatAssistantService.GetConversationsAsync(GetCurrentUserId(), cancellationToken));
    }

    [HttpGet("conversations/{id:int}")]
    public async Task<ActionResult<ConversationDetailResponse>> GetConversation(int id, CancellationToken cancellationToken)
    {
        return Ok(await chatAssistantService.GetConversationAsync(GetCurrentUserId(), id, cancellationToken));
    }

    [HttpPost("conversations")]
    public async Task<ActionResult<ConversationSummaryResponse>> CreateConversation(CreateConversationRequest request, CancellationToken cancellationToken)
    {
        var validation = await createConversationValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new RequestValidationException(validation.ToDictionary());
        }

        var response = await chatAssistantService.CreateConversationAsync(GetCurrentUserId(), request, cancellationToken);
        return CreatedAtAction(nameof(GetConversation), new { id = response.Id }, response);
    }

    [HttpDelete("conversations/{id:int}")]
    public async Task<IActionResult> DeleteConversation(int id, CancellationToken cancellationToken)
    {
        await chatAssistantService.DeleteConversationAsync(GetCurrentUserId(), id, cancellationToken);
        return NoContent();
    }

    private int GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userId, out var value) ? value : 0;
    }
}
