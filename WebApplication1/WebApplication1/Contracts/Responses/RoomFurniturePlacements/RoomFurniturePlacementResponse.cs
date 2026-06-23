namespace WebApplication1.Contracts.Responses;

public class RoomFurniturePlacementResponse
{
    public int Id { get; set; }
    public int RoomDesignId { get; set; }
    public int ProductId { get; set; }
    public decimal PositionX { get; set; }
    public decimal PositionY { get; set; }
    public decimal Rotation { get; set; }
    public decimal Scale { get; set; }
    public int ZIndex { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public ProductSummaryResponse? Product { get; set; }
}
