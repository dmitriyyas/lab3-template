using System.Text.Json.Serialization;

namespace StatsService.Dto;

public class StatDto(int id,
    string method,
    string path,
    int statusCode,
    long durationMs,
    DateTime timestamp)
{
    [JsonPropertyName("id")]
    public int Id { get; set; } = id;

    [JsonPropertyName("method")]
    public string Method { get; set; } = method;

    [JsonPropertyName("path")]
    public string Path { get; set; } = path;

    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; } = statusCode;

    [JsonPropertyName("durationMs")]
    public long DurationMs { get; set; } = durationMs;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = timestamp;
}

