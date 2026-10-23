using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartShop.API.Entities;

[Table("Review")]
public class Review
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ReviewId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [ForeignKey(nameof(ProductId))]
    public Product? Product { get; set; }

    [Required]
    [MaxLength(64)]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Content { get; set; } = string.Empty;

    [Range(1, 5)]
    public int Rating { get; set; }

    [Required]
    [MaxLength(16)]
    public string Sentiment { get; set; } = string.Empty;

    public double Score { get; set; }

    [MaxLength(256)]
    public string Summary { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
