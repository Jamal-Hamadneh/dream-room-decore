namespace WebApplication1.Contracts.Requests;

public class UpdatePlacementRequest
{
    public int PlacementId { get; set; }
    public decimal PositionX { get; set; }
    public decimal PositionY { get; set; }
    public decimal Rotation { get; set; }
    public decimal Scale { get; set; }
    public int ZIndex { get; set; }
}
