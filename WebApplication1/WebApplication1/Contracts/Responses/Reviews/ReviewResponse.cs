namespace WebApplication1.Contracts.Responses;

public class ReviewResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public UserSummaryResponse? User { get; set; }
    public ProductSummaryResponse? Product { get; set; }
}
