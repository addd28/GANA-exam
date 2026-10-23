using System.ComponentModel.DataAnnotations;

namespace SmartShop.API.DTOs;

public class ChatRequest
{
    [Required(ErrorMessage = "SessionId is required.")]
    [StringLength(64, ErrorMessage = "SessionId cannot exceed 64 characters.")]
    public string SessionId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Message is required.")]
    [StringLength(2000, MinimumLength = 1, ErrorMessage = "Message must not be empty.")]
    public string Message { get; set; } = string.Empty;
}
