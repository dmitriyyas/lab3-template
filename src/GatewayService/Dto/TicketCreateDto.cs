﻿using System.Text.Json.Serialization;

namespace GatewayService.Dto;

public class TicketCreateDto(string flightNumber,
    int price)
{
    [JsonPropertyName("flightNumber")]
    public string FlightNumber { get; set; } = flightNumber;

    [JsonPropertyName("price")]
    public int Price { get; set; } = price;
}
