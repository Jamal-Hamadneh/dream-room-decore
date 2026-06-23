namespace WebApplication1.Contracts.Requests;

public class AddressRequest
{
    public int UserId { get; set; }
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string? Building { get; set; }
    public string? PostalCode { get; set; }
    public bool IsDefault { get; set; }
}
