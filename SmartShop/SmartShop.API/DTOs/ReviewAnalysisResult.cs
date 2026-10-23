using System.Text.Json.Serialization;

namespace SmartShop.API.DTOs;

public class ReviewAnalysisResult
{
    [JsonPropertyName("sentiment")]
    public string Sentiment { get; set; } = string.Empty;

    [JsonPropertyName("score")]
    public double Score { get; set; }

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;
}
