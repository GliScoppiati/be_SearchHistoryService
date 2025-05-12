using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SearchHistoryService.Data;
using SearchHistoryService.DTOs;
using SearchHistoryService.Models;
using System.Security.Claims;

namespace SearchHistoryService.Controllers;

[Authorize]
[ApiController]
[Route("api/searchhistory")]
public class UserSearchHistoryController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public UserSearchHistoryController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> SaveHistory([FromBody] SearchHistoryRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var userGuid = Guid.Parse(userId);

        var history = new SearchHistory
        {
            UserId = userGuid,
            Action = request.Action,
            Filters = request.Filters.Select(f => new SearchFilter
            {
                FilterType = f.FilterType,
                FilterName = f.FilterName
            }).ToList()
        };

        _db.SearchHistories.Add(history);
        await _db.SaveChangesAsync();

        return Ok();
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMine([FromQuery] int? limit = null)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var userGuid = Guid.Parse(userId);

        var query = _db.SearchHistories
            .Include(s => s.Filters)
            .Where(s => s.UserId == userGuid)
            .OrderByDescending(s => s.SearchedAt);

        var histories = limit.HasValue
            ? await query.Take(limit.Value).ToListAsync()
            : await query.ToListAsync();

        var result = histories.Select(h => new SearchHistoryDto
        {
            Id         = h.Id,
            SearchedAt = h.SearchedAt,
            Action     = h.Action,
            Filters    = h.Filters.Select(f => new FilterDto
            {
                FilterType = f.FilterType,
                FilterName = f.FilterName
            }).ToList()
        });

        return Ok(result);
    }

    [HttpGet("mine/cocktails")]
    public async Task<IActionResult> GetRecentUserCocktails([FromQuery] bool unique = false)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var userGuid = Guid.Parse(userId);

        var query = _db.SearchHistories
            .Where(h => h.UserId == userGuid && h.Action == "select")
            .OrderByDescending(h => h.SearchedAt)
            .SelectMany(h => h.Filters)
            .Where(f => f.FilterType == "cocktail")
            .Select(f => f.FilterName);

        if (unique)
            query = query.Distinct();

        var recent = await query
            .Take(10)
            .ToListAsync();

        return Ok(recent);
    }

    [HttpGet("mine/stats")]
    public async Task<IActionResult> GetUserStats()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var userGuid = Guid.Parse(userId);

        var filters = await _db.SearchFilters
            .Where(f => f.SearchHistory.UserId == userGuid)
            .ToListAsync();

        var stats = filters
            .GroupBy(f => new { f.FilterType, f.FilterName })
            .Select(g => new
            {
                g.Key.FilterType,
                g.Key.FilterName,
                Count = g.Count()
            })
            .GroupBy(x => x.FilterType)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.Count));

        return Ok(stats);
    }

    [HttpGet("popular-filters")]
    public async Task<IActionResult> GetPopularFilters()
    {
        var filters = await _db.SearchFilters.ToListAsync();

        var stats = filters
            .GroupBy(f => new { f.FilterType, f.FilterName })
            .Select(g => new
            {
                g.Key.FilterType,
                g.Key.FilterName,
                Count = g.Count()
            })
            .GroupBy(x => x.FilterType)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.Count)
            );

        return Ok(stats);
    }

    [HttpGet("recent-filters")]
    public async Task<IActionResult> GetRecentFilters()
    {
        var filters = await _db.SearchFilters
            .OrderByDescending(f => f.SearchHistory.SearchedAt)
            .Take(20)
            .Select(f => new
            {
                f.FilterType,
                f.FilterName,
                f.SearchHistory.SearchedAt
            })
            .ToListAsync();

        return Ok(filters);
    }

    [HttpGet("filter-summary")]
    public async Task<IActionResult> GetFilterSummary()
    {
        var filters = await _db.SearchFilters.ToListAsync();

        var summary = filters
            .GroupBy(f => f.FilterType)
            .Select(g => new
            {
                FilterType = g.Key,
                Count = g.Count()
            });

        return Ok(summary);
    }

    [HttpGet("popular-cocktails")]
    public async Task<IActionResult> GetPopularCocktails()
    {
        var filters = await _db.SearchFilters
            .Where(f => f.FilterType == "cocktail")
            .ToListAsync();

        var popular = filters
            .GroupBy(f => f.FilterName)
            .Select(g => new
            {
                Cocktail = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count);

        return Ok(popular);
    }

    [HttpGet("popular-glasses")]
    public async Task<IActionResult> GetPopularGlasses()
    {
        var filters = await _db.SearchFilters
            .Where(f => f.FilterType == "glass")
            .ToListAsync();

        var popular = filters
            .GroupBy(f => f.FilterName)
            .Select(g => new
            {
                Glass = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count);

        return Ok(popular);
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteAll()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var userGuid = Guid.Parse(userId);

        var histories = await _db.SearchHistories
            .Where(h => h.UserId == userGuid)
            .ToListAsync();

        _db.SearchHistories.RemoveRange(histories);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteById(Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var userGuid = Guid.Parse(userId);

        var history = await _db.SearchHistories.FirstOrDefaultAsync(h => h.Id == id);
        if (history == null || history.UserId != userGuid) return Forbid();

        _db.SearchHistories.Remove(history);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}