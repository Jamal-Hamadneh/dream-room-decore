namespace WebApplication1.Services;

public class RoomAiPromptData
{
    public string RoomImageUrl { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public decimal Height { get; set; }
    public decimal Width { get; set; }
    public decimal Depth { get; set; }
    public List<RoomAiProductData> Products { get; set; } = new();
}

public class RoomAiProductData
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Material { get; set; }
    public string? Color { get; set; }
    public string? ImageUrl { get; set; }
    public decimal PositionX { get; set; }
    public decimal PositionY { get; set; }
    public decimal Rotation { get; set; }
    public decimal Scale { get; set; }
    public int ZIndex { get; set; }
}
