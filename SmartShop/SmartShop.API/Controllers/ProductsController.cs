using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartShop.API.Data;
using SmartShop.API.DTOs;
using SmartShop.API.Exceptions;
using SmartShop.API.Interfaces;

namespace SmartShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly SmartShopDbContext _context;
    private readonly IGeminiService _geminiService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        SmartShopDbContext context,
        IGeminiService geminiService,
        ILogger<ProductsController> logger)
    {
        _context = context;
        _geminiService = geminiService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all products in the catalog.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var products = await _context.Products
            .OrderBy(p => p.ProductId)
            .ToListAsync(cancellationToken);

        return Ok(products);
    }

    /// <summary>
    /// Gets a product by id.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FindAsync([id], cancellationToken);
        if (product == null)
        {
            return NotFound(new { message = $"Product with ID {id} not found." });
        }

        return Ok(product);
    }

    /// <summary>
    /// Question 1: Generate product description using Gemini copywriter prompt.
    /// POST /api/products/{id}/generatedescription
    /// </summary>
    [HttpPost("{id:int}/generatedescription")]
    public async Task<IActionResult> GenerateDescription(int id, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FindAsync([id], cancellationToken);
        if (product == null)
        {
            return NotFound(new { message = $"Product with ID {id} not found." });
        }

        try
        {
            var description = await _geminiService.GenerateProductDescriptionAsync(
                product.ProductName,
                product.Category,
                product.Features,
                cancellationToken);

            product.Description = description;
            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new ProductDescriptionResponse
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Description = product.Description
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Gemini configuration error");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
        catch (GeminiApiException ex)
        {
            _logger.LogError(ex, "Gemini API error during description generation");
            var statusCode = ex.StatusCode.HasValue ? (int)ex.StatusCode.Value : StatusCodes.Status502BadGateway;
            return StatusCode(statusCode, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error generating product description");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while generating the product description." });
        }
    }
}
