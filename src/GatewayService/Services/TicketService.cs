using GatewayService.Dto;
using System.Collections.Generic;

namespace GatewayService.Services;

public class TicketService(IHttpClientFactory httpClientFactory,
    CircuitBreaker circuitBreaker) : IService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly CircuitBreaker _circuitBreaker = circuitBreaker;
    private const string _serviceName = "Ticket";
    public Uri Address { get; } = new("http://ticket_service:8070/");

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

    public async Task<ServiceResponse<List<TicketDto>>> GetTickets(string username)
    {
        if (_circuitBreaker.IsOpen(Address))
            return Fallback<List<TicketDto>>();

        var client = _httpClientFactory.CreateClient();
        using var ticketsRequest = new HttpRequestMessage(HttpMethod.Get, Address + "api/v1/tickets");
        ticketsRequest.Headers.Add("X-User-Name", username);

        return await _circuitBreaker.ExecuteAsync(async () =>
        {
            using var ticketsResponse = await client.SendAsync(ticketsRequest);
            return new ServiceResponse<List<TicketDto>>(await ticketsResponse.Content.ReadFromJsonAsync<List<TicketDto>>());
        }, HealthCheck, Fallback<List<TicketDto>>(), Address);
    }

    public async Task<ServiceResponse<TicketDto>> GetTicket(string username, Guid ticketUid)
    {
        if (_circuitBreaker.IsOpen(Address))
            return Fallback<TicketDto>();

        var client = _httpClientFactory.CreateClient();
        using var ticketRequest = new HttpRequestMessage(HttpMethod.Get, Address + $"api/v1/tickets/{ticketUid}");
        ticketRequest.Headers.Add("X-User-Name", username);

        return await _circuitBreaker.ExecuteAsync(async () =>
        {
            using var ticketResponse = await client.SendAsync(ticketRequest);
            if (!ticketResponse.IsSuccessStatusCode)
                return new ServiceResponse<TicketDto>(null, (int)ticketResponse.StatusCode, new ErrorDto(await ticketResponse.Content.ReadAsStringAsync()));

            return new ServiceResponse<TicketDto>(await ticketResponse.Content.ReadFromJsonAsync<TicketDto>());
        }, HealthCheck, Fallback<TicketDto>(), Address);
    }

    public async Task<ServiceResponse<bool>> CancelTicket(string username, Guid ticketUid)
    {
        if (_circuitBreaker.IsOpen(Address))
            return Fallback<bool>();

        var client = _httpClientFactory.CreateClient();
        using var ticketRequest = new HttpRequestMessage(HttpMethod.Delete, Address + $"api/v1/tickets/{ticketUid}");
        ticketRequest.Headers.Add("X-User-Name", username);

        try
        {
            using var ticketResponse = await client.SendAsync(ticketRequest);
            if (!ticketResponse.IsSuccessStatusCode)
                return new(false, (int)ticketResponse.StatusCode, new ErrorDto(await ticketResponse.Content.ReadAsStringAsync()));

            return new(true, (int)ticketResponse.StatusCode);
        }
        catch (Exception ex)
        {
            return Fallback<bool>();
        }
    }

    public async Task<ServiceResponse<TicketDto>> CreateTicket(string username, TicketCreateDto ticketCreateDto)
    {
        if (_circuitBreaker.IsOpen(Address))
            return Fallback<TicketDto>();

        var client = _httpClientFactory.CreateClient();
        var ticketRequest = new HttpRequestMessage(HttpMethod.Post, Address + $"api/v1/tickets");
        ticketRequest.Headers.Add("X-User-Name", username);
        ticketRequest.Content = JsonContent.Create(ticketCreateDto);

        try
        {
            using var ticketResponse = await client.SendAsync(ticketRequest);
            if (!ticketResponse.IsSuccessStatusCode)
                return new(null, (int)ticketResponse.StatusCode, new ErrorDto(await ticketResponse.Content.ReadAsStringAsync()));

            return new(await ticketResponse.Content.ReadFromJsonAsync<TicketDto>());
        }
        catch (Exception ex)
        {
            return Fallback<TicketDto>();
        }
    }
}
