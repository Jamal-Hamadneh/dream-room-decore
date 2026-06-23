namespace WebApplication1.Models;

public class AIChat
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public ICollection<AIMessage> AIMessages { get; set; } = new List<AIMessage>();
}
