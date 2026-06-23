namespace WebApplication1.Contracts.Requests;

public class ReviewRequest
{
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}
