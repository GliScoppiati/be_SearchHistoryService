using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SearchHistoryService.Models;

public class SearchFilter
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SearchHistoryId { get; set; }

    [ForeignKey("SearchHistoryId")]
    public SearchHistory? SearchHistory { get; set; }

    public string FilterType { get; set; } = string.Empty;
    public string FilterName { get; set; } = string.Empty;
}