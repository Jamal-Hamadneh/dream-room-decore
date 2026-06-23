namespace WebApplication1.Contracts.Responses;

public class AiRoomProductResponse
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Material { get; set; }
    public string? Color { get; set; }
    public decimal? Height { get; set; }
    public decimal? Width { get; set; }
    public decimal? Depth { get; set; }
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }
}
