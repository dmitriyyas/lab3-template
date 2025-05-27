using System.Text.Json.Serialization;

namespace AuthService.Dto;

public class UserDto(int id,
    string username,
    string name,
    string surname,
    string fathername,
    string phone,
    string email,
    string role)
{
    [JsonPropertyName("id")]
    public int Id { get; set; } = id;

    [JsonPropertyName("username")]
    public string Username { get; set; } = username;

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

    [JsonPropertyName("role")]
    public string Role { get; set; } = role;
}
