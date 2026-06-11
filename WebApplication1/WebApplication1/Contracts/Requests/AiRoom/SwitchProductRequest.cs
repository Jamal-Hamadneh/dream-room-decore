namespace WebApplication1.Contracts.Requests;

public class SwitchProductRequest
{
    public int RoomDesignId { get; set; }
    public int OldProductId { get; set; }
    public int NewProductId { get; set; }
}
