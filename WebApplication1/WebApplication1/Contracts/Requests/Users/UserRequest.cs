namespace WebApplication1.Contracts.Requests;

public class UserRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? ProfileImage { get; set; }
    public string Role { get; set; } = "customer";
}
