using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SearchHistoryService.Data;

namespace SearchHistoryService.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/searchhistory/global")]
public class AdminSearchHistoryController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public AdminSearchHistoryController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetGlobalStats()
    {
        var filters = await _db.SearchFilters
            .Include(f => f.SearchHistory)
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
}
