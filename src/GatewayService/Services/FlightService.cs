
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
    private const string _serviceName = "Flight";

    public Uri Address { get; } = new("http://flight_service:8060/");

    private static ServiceResponse<T> Fallback<T>() => ServiceResponse<T>.Fallback(_serviceName);

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
            return Fallback<bool>();
        }
    }

    public async Task<ServiceResponse<FlightsDto>> GetFlights(List<string>? flightNumbers = null, int? page = null, int? size = null)
    {
        if (_circuitBreaker.IsOpen(Address))
            return Fallback<FlightsDto>();

        var client = _httpClientFactory.CreateClient();
        var query = "api/v1/flights";
        if (flightNumbers is not null || page is not null || size is not null)
            query += '?';
        if (flightNumbers is not null)
            query += string.Join('&', flightNumbers.Select(num => $"numbers={num}"));
        if (page is not null && size is not null)
            query += $"page={page}&size={size}";

        using var request = new HttpRequestMessage(HttpMethod.Get, Address + query);

        return await _circuitBreaker.ExecuteAsync(async () =>
        {
            using var response = await client.SendAsync(request);
            return new ServiceResponse<FlightsDto>(await response.Content.ReadFromJsonAsync<FlightsDto>());
        }, HealthCheck, Fallback<FlightsDto>(), Address);
    }

    public async Task<ServiceResponse<FlightDto>> GetFlight(string flightNumber)
    {
        if (_circuitBreaker.IsOpen(Address))
            return Fallback<FlightDto>();

        var client = _httpClientFactory.CreateClient();
        var flightRequest = new HttpRequestMessage(HttpMethod.Get, Address + $"api/v1/flights/{flightNumber}");

        return await _circuitBreaker.ExecuteAsync(async () =>
        {
            using var flightResponse = await client.SendAsync(flightRequest);
            if (!flightResponse.IsSuccessStatusCode)
                return new ServiceResponse<FlightDto>(null, (int)flightResponse.StatusCode, new ErrorDto(await flightResponse.Content.ReadAsStringAsync()));
            return new ServiceResponse<FlightDto>(await flightResponse.Content.ReadFromJsonAsync<FlightDto>());
        }, HealthCheck, Fallback<FlightDto>(), Address);
    }
}
