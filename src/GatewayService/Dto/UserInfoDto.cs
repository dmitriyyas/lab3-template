using System.Text.Json.Serialization;

namespace GatewayService.Dto;

public class UserInfoDto(List<TicketDetailDto> tickets,
    object privilege)
{
    [JsonPropertyName("tickets")]
    public List<TicketDetailDto> Tickets { get; set; } = tickets;

    [JsonPropertyName("privilege")]
    public object privilege { get; set; } = privilege;
}
