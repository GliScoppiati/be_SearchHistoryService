using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SearchHistoryService.Models;

public class SearchHistory
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public DateTime SearchedAt { get; set; } = DateTime.UtcNow;
    public string Action { get; set; } = "search";
    public List<SearchFilter> Filters { get; set; } = new();
}