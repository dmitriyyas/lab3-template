using System.Text.Json.Serialization;

namespace AuthService.Dto;

public class TokenRequestDto(string code,
    string clientId)
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = code;

    [JsonPropertyName("clientId")]
    public string ClientId { get; set; } = clientId;
}

