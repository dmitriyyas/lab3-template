using System.Text.Json.Serialization;

namespace StatsService.Dto;

public class StatsDto(int page,
    int pageSize,
    int totalElements,
    List<StatDto> items)
{
    [JsonPropertyName("page")]
    public int Page { get; set; } = page;

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; } = pageSize;

    [JsonPropertyName("totalElements")]
    public int TotalElements { get; set; } = totalElements;

    [JsonPropertyName("items")]
    public List<StatDto> Items { get; set; } = items;
}
