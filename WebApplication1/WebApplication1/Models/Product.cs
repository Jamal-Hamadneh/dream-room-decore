namespace WebApplication1.Models;

public class Product
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int StockQuantity { get; set; }
    public string? Material { get; set; }
    public string? Color { get; set; }
    public decimal? Height { get; set; }
    public decimal? Width { get; set; }
    public decimal? Depth { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewsCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Category Category { get; set; } = null!;
    public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    public ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<RoomFurniturePlacement> RoomFurniturePlacements { get; set; } = new List<RoomFurniturePlacement>();
}
