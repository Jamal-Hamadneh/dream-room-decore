namespace WebApplication1.Models;

public class RevokedAccessToken
{
    public int Id { get; set; }
    public string JwtId { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime RevokedAt { get; set; }
}
