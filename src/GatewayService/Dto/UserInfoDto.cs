using System.Text.Json.Serialization;

namespace GatewayService.Dto;

public class UserInfoDto(UserDto userDto,
    List<TicketDetailDto> tickets,
    ShortPrivilegeDto? privilege)
{
    [JsonPropertyName("user")]
    public UserDto User { get; set; } = userDto;

    [JsonPropertyName("tickets")]
    public List<TicketDetailDto> Tickets { get; set; } = tickets;

    [JsonPropertyName("privilege")]
    public ShortPrivilegeDto? Privilege { get; set; } = privilege;
}
