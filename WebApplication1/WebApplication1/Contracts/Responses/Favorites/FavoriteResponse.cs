namespace WebApplication1.Contracts.Responses;

public class FavoriteResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public DateTime CreatedAt { get; set; }
    public ProductSummaryResponse? Product { get; set; }
}
