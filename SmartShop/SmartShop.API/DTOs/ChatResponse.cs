namespace SmartShop.API.DTOs;

public class ChatResponse
{
    public string SessionId { get; set; } = string.Empty;
    public string Reply { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
