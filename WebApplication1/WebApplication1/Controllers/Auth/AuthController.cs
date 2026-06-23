using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Exceptions;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    IAuthService authService,
    IValidator<RegisterRequest> registerValidator,
    IValidator<LoginRequest> loginValidator,
    IValidator<RefreshTokenRequest> refreshTokenValidator,
    IValidator<RevokeRefreshTokenRequest> revokeRefreshTokenValidator) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var validation = await registerValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new RequestValidationException(validation.ToDictionary());
        }

        var response = await authService.RegisterAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var validation = await loginValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new RequestValidationException(validation.ToDictionary());
        }

        var response = await authService.LoginAsync(request, cancellationToken);
        return response is null ? throw new InvalidCredentialsException() : Ok(response);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var validation = await refreshTokenValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new RequestValidationException(validation.ToDictionary());
        }

        return Ok(await authService.RefreshAsync(request, cancellationToken));
    }

    [HttpPost("revoke")]
    [AllowAnonymous]
    public async Task<IActionResult> Revoke(RevokeRefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var validation = await revokeRefreshTokenValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new RequestValidationException(validation.ToDictionary());
        }

        await authService.RevokeRefreshTokenAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPost("logout")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Logout(LogoutRequest request, CancellationToken cancellationToken)
    {
        var accessToken = Request.Headers.Authorization.ToString().Replace("Bearer ", string.Empty, StringComparison.OrdinalIgnoreCase);
        await authService.LogoutAsync(accessToken, request.RefreshToken, cancellationToken);
        return NoContent();
    }
}
