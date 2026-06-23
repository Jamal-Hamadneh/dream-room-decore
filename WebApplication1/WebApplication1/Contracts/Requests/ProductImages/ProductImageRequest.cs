namespace WebApplication1.Contracts.Requests;

public class ProductImageRequest
{
    public int ProductId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsMain { get; set; }
}
