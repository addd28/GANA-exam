using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartShop.API.Data;
using SmartShop.API.DTOs;
using SmartShop.API.Entities;
using SmartShop.API.Exceptions;
using SmartShop.API.Interfaces;

namespace SmartShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly SmartShopDbContext _context;
    private readonly IGeminiService _geminiService;
    private readonly ILogger<ReviewsController> _logger;

    public ReviewsController(
        SmartShopDbContext context,
        IGeminiService geminiService,
        ILogger<ReviewsController> logger)
    {
        _context = context;
        _geminiService = geminiService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all reviews.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var reviews = await _context.Reviews
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewResponse
            {
                ReviewId = r.ReviewId,
                ProductId = r.ProductId,
                CustomerName = r.CustomerName,
                Content = r.Content,
                Rating = r.Rating,
                Sentiment = r.Sentiment,
                Score = r.Score,
                Summary = r.Summary,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(reviews);
    }

    /// <summary>
    /// Question 2: Analyze customer review with Gemini structured JSON output and persist to SQL Server.
    /// POST /api/reviews/analyze
    /// </summary>
    [HttpPost("analyze")]
    public async Task<IActionResult> AnalyzeReview([FromBody] AnalyzeReviewRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var product = await _context.Products.FindAsync([request.ProductId], cancellationToken);
        if (product == null)
        {
            return NotFound(new { message = $"Product with ID {request.ProductId} not found." });
        }

        try
        {
            // Perform Gemini structured sentiment analysis
            var analysisResult = await _geminiService.AnalyzeReviewAsync(
                product.ProductName,
                request.Content,
                request.Rating,
                cancellationToken);

            // Create and persist the review entity
            var review = new Review
            {
                ProductId = request.ProductId,
                CustomerName = request.CustomerName.Trim(),
                Content = request.Content.Trim(),
                Rating = request.Rating,
                Sentiment = analysisResult.Sentiment,
                Score = analysisResult.Score,
                Summary = analysisResult.Summary,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync(cancellationToken);

            var response = new ReviewResponse
            {
                ReviewId = review.ReviewId,
                ProductId = review.ProductId,
                CustomerName = review.CustomerName,
                Content = review.Content,
                Rating = review.Rating,
                Sentiment = review.Sentiment,
                Score = review.Score,
                Summary = review.Summary,
                CreatedAt = review.CreatedAt
            };

            return CreatedAtAction(nameof(GetAll), new { id = review.ReviewId }, response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Gemini configuration error");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
        catch (GeminiApiException ex)
        {
            _logger.LogError(ex, "Gemini analysis error");
            var statusCode = ex.StatusCode.HasValue ? (int)ex.StatusCode.Value : StatusCodes.Status502BadGateway;
            return StatusCode(statusCode, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error analyzing customer review");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while analyzing the review." });
        }
    }
}
