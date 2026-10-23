using SmartShop.API.DTOs;

namespace SmartShop.API.Interfaces;

public interface IGeminiService
{
    Task<string> GenerateProductDescriptionAsync(string productName, string category, string features, CancellationToken cancellationToken = default);
    Task<ReviewAnalysisResult> AnalyzeReviewAsync(string productName, string content, int rating, CancellationToken cancellationToken = default);
    Task<string> ChatAssistantAsync(string productCatalog, IEnumerable<(string role, string content)> history, string userMessage, CancellationToken cancellationToken = default);
}
