namespace WebApplication1.Contracts.Responses;

public class ChatbotContextResponse
{
    public ChatbotUserSummary User { get; set; } = new();
    public List<ChatbotCartItemSummary> CartItems { get; set; } = new();
    public List<ChatbotOrderSummary> RecentOrders { get; set; } = new();
    public List<ChatbotRoomDesignSummary> RoomDesigns { get; set; } = new();
}

public class ChatbotUserSummary
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class ChatbotCartItemSummary
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string? VariantSku { get; set; }
}

public class ChatbotOrderSummary
{
    public int OrderId { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ChatbotRoomDesignSummary
{
    public int RoomDesignId { get; set; }
    public int RoomUploadId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
