namespace SearchHistoryService.DTOs;

public class SearchHistoryDto
{
    public Guid Id { get; set; } 
    public DateTime SearchedAt { get; set; }
    public string Action { get; set; } = string.Empty;
    public List<FilterDto> Filters { get; set; } = new();
}