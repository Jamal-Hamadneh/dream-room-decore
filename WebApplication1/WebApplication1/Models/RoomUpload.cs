namespace WebApplication1.Models;

public class RoomUpload
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public decimal Height { get; set; }
    public decimal Width { get; set; }
    public decimal Depth { get; set; }
    public decimal? AiDetectedWidth { get; set; }
    public decimal? AiDetectedHeight { get; set; }
    public decimal? AiDetectedDepth { get; set; }
    public string? AiDescription { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public ICollection<RoomDesign> RoomDesigns { get; set; } = new List<RoomDesign>();
}
