using System.Text.Json.Serialization;

namespace GatewayService.Dto;

public class AuthDataDto(string username,
    string password,
    string redirectUri,
    string clientId,
    string state,
    string scope)
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = username;

    [JsonPropertyName("password")]
    public string Password { get; set; } = password;

    [JsonPropertyName("redirectUri")]
    public string RedirectUri { get; set; } = redirectUri;

    [JsonPropertyName("clientId")]
    public string ClientId { get; set; } = clientId;

    [JsonPropertyName("state")]
    public string State { get; set; } = state;

    [JsonPropertyName("scope")]
    public string Scope { get; set; } = scope;
}
