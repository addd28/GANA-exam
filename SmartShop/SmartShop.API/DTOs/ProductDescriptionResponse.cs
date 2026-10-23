namespace SmartShop.API.DTOs;

public class ProductDescriptionResponse
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
