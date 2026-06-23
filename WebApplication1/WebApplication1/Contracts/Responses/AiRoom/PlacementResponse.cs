namespace WebApplication1.Contracts.Responses;

public class PlacementResponse
{
    public int Id { get; set; }
    public int RoomDesignId { get; set; }
    public int ProductId { get; set; }
    public decimal PositionX { get; set; }
    public decimal PositionY { get; set; }
    public decimal Rotation { get; set; }
    public decimal Scale { get; set; }
    public int ZIndex { get; set; }
}
