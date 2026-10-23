using System.ComponentModel.DataAnnotations;

namespace SmartShop.API.DTOs;

public class AnalyzeReviewRequest
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "ProductId must be a valid positive integer.")]
    public int ProductId { get; set; }

    [Required(ErrorMessage = "CustomerName is required.")]
    [StringLength(64, ErrorMessage = "CustomerName cannot exceed 64 characters.")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content is required.")]
    [StringLength(1000, MinimumLength = 3, ErrorMessage = "Content must be between 3 and 1000 characters.")]
    public string Content { get; set; } = string.Empty;

    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    public int Rating { get; set; }
}
