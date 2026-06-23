namespace WebApplication1.Models;

public class RoomDesignReplacement
{
    public int Id { get; set; }
    public int RoomDesignId { get; set; }
    public int OldProductId { get; set; }
    public int NewProductId { get; set; }
    public string Instruction { get; set; } = string.Empty;
    public string? GeneratedImageUrl { get; set; }
    public string Status { get; set; } = "completed";
    public DateTime CreatedAt { get; set; }

    public RoomDesign RoomDesign { get; set; } = null!;
    public Product OldProduct { get; set; } = null!;
    public Product NewProduct { get; set; } = null!;
}
