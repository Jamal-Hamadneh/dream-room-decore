namespace WebApplication1.Contracts.Responses;

public class AddressResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string? Building { get; set; }
    public string? PostalCode { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
}
