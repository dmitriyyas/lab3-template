using System.Text.Json.Serialization;

namespace GatewayService.Dto;

public class UserCreateDto(string username,
    string password,
    string name,
    string surname,
    string fathername,
    string phone,
    string email)
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = username;

    [JsonPropertyName("password")]
    public string Password { get; set; } = password;

    [JsonPropertyName("name")]
    public string Name { get; set; } = name;

    [JsonPropertyName("surname")]
    public string Surname { get; set; } = surname;

    [JsonPropertyName("fathername")]
    public string Fathername { get; set; } = fathername;

    [JsonPropertyName("phone")]
    public string Phone { get; set; } = phone;

    [JsonPropertyName("email")]
    public string Email { get; set; } = email;
}
