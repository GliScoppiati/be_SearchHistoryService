namespace SearchHistoryService.DTOs;

public class SearchHistoryRequest
{
    public List<FilterDto> Filters { get; set; } = new();
    public string Action { get; set; } = "search";
}

public class FilterDto
{
    public string FilterType { get; set; } = string.Empty;
    public string FilterName { get; set; } = string.Empty;
}