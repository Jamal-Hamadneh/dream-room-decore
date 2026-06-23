namespace WebApplication1.Contracts.Requests;

public class RevokeRefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
