using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebApplication1.Contracts.Requests;
using WebApplication1.Contracts.Responses;
using WebApplication1.Data;
using WebApplication1.Exceptions.Auth;
using WebApplication1.Exceptions.Users;
using WebApplication1.Models;
using WebApplication1.Options;

namespace WebApplication1.Services;

public class AuthService(
    ApplicationDbContext context,
    PasswordHasher<User> passwordHasher,
    IOptions<JwtOptions> jwtOptions) : IAuthService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var exists = await context.Users.AnyAsync(user => user.Email == email, cancellationToken);
        if (exists)
        {
            throw new UserEmailAlreadyExistsException(email);
        }

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            Phone = request.Phone,
            ProfileImage = request.ProfileImage,
            Role = request.Role,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        return await CreateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await context.Users.FirstOrDefaultAsync(user => user.Email == email, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }

        return await CreateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(request.RefreshToken);
        var refreshToken = await context.RefreshTokens
            .Include(token => token.User)
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken)
            ?? throw new InvalidRefreshTokenException();

        if (!refreshToken.IsActive)
        {
            throw new InvalidRefreshTokenException("Refresh token is expired or revoked.");
        }

        return await CreateAuthResponseAsync(refreshToken.User, cancellationToken, refreshToken);
    }

    public async Task RevokeRefreshTokenAsync(RevokeRefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(request.RefreshToken);
        var refreshToken = await context.RefreshTokens.FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken)
            ?? throw new InvalidRefreshTokenException();

        if (refreshToken.RevokedAt is null)
        {
            refreshToken.RevokedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task LogoutAsync(string accessToken, string? refreshToken, CancellationToken cancellationToken = default)
    {
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
        var jwtId = jwt.Id;

        if (!string.IsNullOrWhiteSpace(jwtId))
        {
            var alreadyRevoked = await context.RevokedAccessTokens.AnyAsync(token => token.JwtId == jwtId, cancellationToken);
            if (!alreadyRevoked)
            {
                context.RevokedAccessTokens.Add(new RevokedAccessToken
                {
                    JwtId = jwtId,
                    ExpiresAt = jwt.ValidTo,
                    RevokedAt = DateTime.UtcNow
                });
            }
        }

        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            var refreshTokenHash = HashToken(refreshToken);
            var storedRefreshToken = await context.RefreshTokens.FirstOrDefaultAsync(token => token.TokenHash == refreshTokenHash, cancellationToken);
            if (storedRefreshToken is not null && storedRefreshToken.RevokedAt is null)
            {
                storedRefreshToken.RevokedAt = DateTime.UtcNow;
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task<AuthResponse> CreateAuthResponseAsync(User user, CancellationToken cancellationToken, RefreshToken? tokenToReplace = null)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiresInMinutes);
        var refreshToken = GenerateRefreshToken();
        var refreshTokenHash = HashToken(refreshToken);
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiresInDays);

        if (tokenToReplace is not null)
        {
            tokenToReplace.RevokedAt = DateTime.UtcNow;
            tokenToReplace.ReplacedByTokenHash = refreshTokenHash;
        }

        context.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = refreshTokenExpiresAt,
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role,
            Token = GenerateToken(user, expiresAt),
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            RefreshTokenExpiresAt = refreshTokenExpiresAt
        };
    }

    private string GenerateToken(User user, DateTime expiresAt)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
