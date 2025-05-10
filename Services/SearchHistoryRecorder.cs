using SearchHistoryService.DTOs;
using SearchHistoryService.Data;
using SearchHistoryService.Models;

namespace SearchHistoryService.Services;

public class SearchHistoryRecorder
{
    private readonly ApplicationDbContext _db;

    public SearchHistoryRecorder(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task SaveSearchAsync(SearchHistoryRequest request, Guid userId)
    {
        var history = new SearchHistory
        {
            UserId = userId,
            Action = request.Action,
            Filters = request.Filters.Select(f => new SearchFilter
            {
                FilterType = f.FilterType,
                FilterName = f.FilterName
            }).ToList()
        };

        _db.SearchHistories.Add(history);
        await _db.SaveChangesAsync();
    }
}
