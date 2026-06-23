using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/stripe")]
public class StripeController(IStripePaymentService stripePaymentService) : ControllerBase
{
    [HttpGet("config")]
    [AllowAnonymous]
    public ActionResult<StripeConfigResponse> GetConfig()
    {
        return Ok(stripePaymentService.GetConfig());
    }

    [HttpPost("payment-intents")]
    [Authorize]
    public Task<CreatePaymentIntentResponse> CreatePaymentIntent(CreatePaymentIntentRequest request, CancellationToken cancellationToken)
    {
        return stripePaymentService.CreatePaymentIntentAsync(GetCurrentUserId(), request, cancellationToken);
    }

    [HttpPost("sync-payment-intent")]
    [Authorize]
    public Task<SyncPaymentIntentResponse> SyncPaymentIntent(SyncPaymentIntentRequest request, CancellationToken cancellationToken)
    {
        return stripePaymentService.SyncPaymentIntentAsync(GetCurrentUserId(), request, cancellationToken);
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var json = await reader.ReadToEndAsync(cancellationToken);
        await stripePaymentService.HandleWebhookAsync(json, Request.Headers["Stripe-Signature"], cancellationToken);
        return Ok();
    }

    private int GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userId, out var value) ? value : 0;
    }
}
