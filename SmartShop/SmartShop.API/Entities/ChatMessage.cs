using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartShop.API.Entities;

[Table("ChatMessage")]
public class ChatMessage
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(64)]
    public string SessionId { get; set; } = string.Empty;

    [Required]
    [MaxLength(16)]
    public string Role { get; set; } = string.Empty; // "user" or "model" / "assistant"

    [Required]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
