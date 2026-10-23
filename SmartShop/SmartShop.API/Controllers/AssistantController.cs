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
public class AssistantController : ControllerBase
{
    private readonly SmartShopDbContext _context;
    private readonly IGeminiService _geminiService;
    private readonly ILogger<AssistantController> _logger;

    public AssistantController(
        SmartShopDbContext context,
        IGeminiService geminiService,
        ILogger<AssistantController> logger)
    {
        _context = context;
        _geminiService = geminiService;
        _logger = logger;
    }

    /// <summary>
    /// Gets conversation history for a given sessionId.
    /// </summary>
    [HttpGet("history/{sessionId}")]
    public async Task<IActionResult> GetHistory(string sessionId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return BadRequest(new { message = "SessionId is required." });
        }

        var history = await _context.ChatMessages
            .Where(m => m.SessionId == sessionId)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new
            {
                m.Id,
                m.SessionId,
                m.Role,
                m.Content,
                m.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(history);
    }

    /// <summary>
    /// Clears conversation history for a given sessionId.
    /// </summary>
    [HttpDelete("history/{sessionId}")]
    public async Task<IActionResult> ClearHistory(string sessionId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return BadRequest(new { message = "SessionId is required." });
        }

        var messages = await _context.ChatMessages
            .Where(m => m.SessionId == sessionId)
            .ToListAsync(cancellationToken);

        _context.ChatMessages.RemoveRange(messages);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { message = $"Cleared {messages.Count} messages for session {sessionId}." });
    }

    /// <summary>
    /// Question 3: AI Shopping Assistant chat with context from catalog and session history.
    /// POST /api/assistant/chat
    /// </summary>
    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var sessionId = request.SessionId.Trim();
        var userText = request.Message.Trim();

        // 1. Load the current product catalog from SQL Server as strict ground truth
        var products = await _context.Products
            .OrderBy(p => p.ProductId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var catalogLines = products.Select(p =>
            $"- [ID: {p.ProductId}] {p.ProductName} | Category: {p.Category} | Price: {p.Price:N0} VND | Features: {p.Features}" +
            (string.IsNullOrWhiteSpace(p.Description) ? "" : $" | Description: {p.Description}"));

        var catalogContext = string.Join("\n", catalogLines);

        // 2. Load prior conversation history bounded to last 10 messages for this specific session
        var priorMessages = await _context.ChatMessages
            .Where(m => m.SessionId == sessionId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(10)
            .OrderBy(m => m.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var historyTuples = priorMessages.Select(m => (m.Role, m.Content));

        // 3. Call Gemini Shopping Assistant
        string assistantReply;
        try
        {
            assistantReply = await _geminiService.ChatAssistantAsync(
                catalogContext,
                historyTuples,
                userText,
                cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Gemini configuration error");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
        catch (GeminiApiException ex)
        {
            _logger.LogError(ex, "Gemini chat assistant error");
            var statusCode = ex.StatusCode.HasValue ? (int)ex.StatusCode.Value : StatusCodes.Status502BadGateway;
            return StatusCode(statusCode, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during assistant chat");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while communicating with the shopping assistant." });
        }

        // 4. Save both user message and assistant reply to ChatMessage table
        var now = DateTime.UtcNow;
        var userMessageEntity = new ChatMessage
        {
            SessionId = sessionId,
            Role = "user",
            Content = userText,
            CreatedAt = now
        };

        var assistantMessageEntity = new ChatMessage
        {
            SessionId = sessionId,
            Role = "model",
            Content = assistantReply,
            CreatedAt = now.AddMilliseconds(100)
        };

        _context.ChatMessages.AddRange(userMessageEntity, assistantMessageEntity);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new ChatResponse
        {
            SessionId = sessionId,
            Reply = assistantReply,
            CreatedAt = assistantMessageEntity.CreatedAt
        });
    }
}
