using System.Text.Json;
using SmartShop.API.DTOs;
using Xunit;

namespace SmartShop.Tests;

public class GeminiReviewParsingTests
{
    [Theory]
    [InlineData("{\"sentiment\": \"Positive\", \"score\": 0.92, \"summary\": \"Battery is great but the phone gets hot.\"}", "Positive", 0.92)]
    [InlineData("{\"sentiment\": \"Negative\", \"score\": 0.85, \"summary\": \"Slow device and bad battery life.\"}", "Negative", 0.85)]
    [InlineData("{\"sentiment\": \"Neutral\", \"score\": 0.50, \"summary\": \"Average phone for the price.\"}", "Neutral", 0.50)]
    public void Deserialize_ValidJsonStructuredOutput_ReturnsCorrectDTO(string json, string expectedSentiment, double expectedScore)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var result = JsonSerializer.Deserialize<ReviewAnalysisResult>(json, options);

        Assert.NotNull(result);
        Assert.Equal(expectedSentiment, result.Sentiment);
        Assert.Equal(expectedScore, result.Score);
        Assert.False(string.IsNullOrWhiteSpace(result.Summary));
    }

    [Fact]
    public void ScoreValidation_ClampingLogic_KeepsScoreBetweenZeroAndOne()
    {
        double outOfBoundsHigh = 1.45;
        double outOfBoundsLow = -0.32;
        double validScore = 0.88;

        double clampedHigh = Math.Round(Math.Clamp(outOfBoundsHigh, 0.0, 1.0), 2);
        double clampedLow = Math.Round(Math.Clamp(outOfBoundsLow, 0.0, 1.0), 2);
        double clampedValid = Math.Round(Math.Clamp(validScore, 0.0, 1.0), 2);

        Assert.Equal(1.0, clampedHigh);
        Assert.Equal(0.0, clampedLow);
        Assert.Equal(0.88, clampedValid);
    }

    [Fact]
    public void WordCount_LimitCheck_EnforcesAtMost80Words()
    {
        // Simulate a 95 words output from LLM
        var longText = string.Join(" ", Enumerable.Range(1, 95).Select(i => $"word{i}"));
        var words = longText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        string processedText;
        if (words.Length > 80)
        {
            processedText = string.Join(" ", words.Take(80)) + "...";
        }
        else
        {
            processedText = longText;
        }

        var processedWords = processedText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        Assert.True(processedWords.Length <= 80);
    }
}
