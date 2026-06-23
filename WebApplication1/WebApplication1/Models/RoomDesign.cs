namespace WebApplication1.Models;

public class RoomDesign
{
    public int Id { get; set; }
    public int RoomUploadId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public RoomUpload RoomUpload { get; set; } = null!;
    public ICollection<RoomFurniturePlacement> RoomFurniturePlacements { get; set; } = new List<RoomFurniturePlacement>();
    public ICollection<GeneratedRoomImage> GeneratedRoomImages { get; set; } = new List<GeneratedRoomImage>();
    public ICollection<RoomDesignReplacement> RoomDesignReplacements { get; set; } = new List<RoomDesignReplacement>();
}
