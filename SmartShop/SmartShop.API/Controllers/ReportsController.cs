using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartShop.API.Data;
using SmartShop.API.DTOs;

namespace SmartShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly SmartShopDbContext _context;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(SmartShopDbContext context, ILogger<ReportsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Question 4: Review report aggregating sentiment and average rating per product.
    /// GET /api/reports/reviews
    /// </summary>
    [HttpGet("reviews")]
    public async Task<IActionResult> GetReviewReport(CancellationToken cancellationToken)
    {
        // Efficient LINQ query joining Products with their Reviews (including products without reviews)
        var productsWithReviews = await _context.Products
            .OrderBy(p => p.ProductId)
            .Select(p => new
            {
                p.ProductId,
                p.ProductName,
                TotalReviews = p.Reviews.Count,
                Positive = p.Reviews.Count(r => r.Sentiment == "Positive"),
                Negative = p.Reviews.Count(r => r.Sentiment == "Negative"),
                Neutral = p.Reviews.Count(r => r.Sentiment == "Neutral"),
                AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => (double)r.Rating) : 0.0
            })
            .ToListAsync(cancellationToken);

        var report = productsWithReviews.Select((item, index) => new ReviewReportItem
        {
            No = index + 1,
            ProductName = item.ProductName,
            TotalReviews = item.TotalReviews,
            Positive = item.Positive,
            Negative = item.Negative,
            Neutral = item.Neutral,
            AverageRating = Math.Round(item.AverageRating, 1)
        }).ToList();

        return Ok(report);
    }
}
