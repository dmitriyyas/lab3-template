
using GatewayService.Dto;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Drawing;
using System.Net.Http;

namespace GatewayService.Services;

public class FlightService(IHttpClientFactory httpClientFactory,
    CircuitBreaker circuitBreaker) : IService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly CircuitBreaker _circuitBreaker = circuitBreaker;

    public Uri Address { get; } = new("http://ticket_service:8070/");

    public async Task<ServiceResponse<bool>> HealthCheck()
    {
        var client = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, Address + "manage/health");
        try
        {
            using var response = await client.SendAsync(request);
            return new(true);
        }
        catch (Exception ex)
        {
            return new(false, 503, ErrorDto.ServiceUnavailable);
        }
    }

    public async Task<ServiceResponse<FlightsDto>> GetFlights(List<string>? flightNumbers = null, int? page = null, int? size = null)
    {
        if (_circuitBreaker.IsOpen(Address))
            return ServiceResponse<FlightsDto>.Fallback;

        var client = _httpClientFactory.CreateClient();
        var query = "api/v1/flights";
        if (flightNumbers is not null)
            query += string.Join('&', flightNumbers.Select(num => $"numbers={num}"));
        if (page is not null && size is not null)
            query += $"?page={page}&size={size}";

        using var request = new HttpRequestMessage(HttpMethod.Get, Address + query);

        return await _circuitBreaker.ExecuteAsync(async () =>
        {
            using var response = await client.SendAsync(request);
            return new ServiceResponse<FlightsDto>(await response.Content.ReadFromJsonAsync<FlightsDto>());
        }, HealthCheck, Address);
    }

    public async Task<ServiceResponse<FlightDto>> GetFlight(string flightNumber)
    {
        if (_circuitBreaker.IsOpen(Address))
            return ServiceResponse<FlightDto>.Fallback;

        var client = _httpClientFactory.CreateClient();
        var flightRequest = new HttpRequestMessage(HttpMethod.Get, Address + $"api/v1/flights/{flightNumber}");

        return await _circuitBreaker.ExecuteAsync(async () =>
        {
            using var flightResponse = await client.SendAsync(flightRequest);
            if (!flightResponse.IsSuccessStatusCode)
                return new ServiceResponse<FlightDto>(null, (int)flightResponse.StatusCode, new ErrorDto(await flightResponse.Content.ReadAsStringAsync()));
            return new ServiceResponse<FlightDto>(await flightResponse.Content.ReadFromJsonAsync<FlightDto>());
        }, HealthCheck, Address);
    }
}
