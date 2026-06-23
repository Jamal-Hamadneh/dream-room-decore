namespace WebApplication1.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
