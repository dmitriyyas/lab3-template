﻿using GatewayService.Dto;
using System.Net.Http;

namespace GatewayService.Services;

public class BonusService(IHttpClientFactory httpClientFactory,
    CircuitBreaker circuitBreaker) : IService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly CircuitBreaker _circuitBreaker = circuitBreaker;
    public Uri Address { get; } = new("http://bonus_service:8050/");

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

    public async Task<ServiceResponse<PrivilegeDto>> GetPrivileges(string username)
    {
        if (_circuitBreaker.IsOpen(Address))
            return ServiceResponse<PrivilegeDto>.Fallback;

        var client = _httpClientFactory.CreateClient();
        using var privilegeRequest = new HttpRequestMessage(HttpMethod.Get, Address + "api/v1/privileges");
        privilegeRequest.Headers.Add("X-User-Name", username);

        return await _circuitBreaker.ExecuteAsync(async () =>
        {
            using var privilegeResponse = await client.SendAsync(privilegeRequest);
            return new ServiceResponse<PrivilegeDto>(await privilegeResponse.Content.ReadFromJsonAsync<PrivilegeDto>());
        }, HealthCheck, Address);
    }

    public async Task<ServiceResponse<PurchaseInfoDto>> BuyTicket(string username, TicketInfoDto ticketInfo)
    {
        if (_circuitBreaker.IsOpen(Address))
            return ServiceResponse<PurchaseInfoDto>.Fallback;

        var client = _httpClientFactory.CreateClient();
        using var buyRequest = new HttpRequestMessage(HttpMethod.Post, Address + $"api/v1/privileges");
        buyRequest.Headers.Add("X-User-Name", username);
        buyRequest.Content = JsonContent.Create(ticketInfo);

        try
        {
            using var buyResponse = await client.SendAsync(buyRequest);
            if (!buyResponse.IsSuccessStatusCode)
                return new(null, (int)buyResponse.StatusCode, new ErrorDto(await buyResponse.Content.ReadAsStringAsync()));
            return new(await buyResponse.Content.ReadFromJsonAsync<PurchaseInfoDto>());
        }
        catch (Exception ex)
        {
            return new(null, 503, ErrorDto.ServiceUnavailable);
        }
    }

    public async Task<ServiceResponse<bool>> CancelTicket(string username, Guid ticketUid)
    {
        if (_circuitBreaker.IsOpen(Address))
            return ServiceResponse<bool>.Fallback;

        var client = _httpClientFactory.CreateClient();
        using var privilegeRequest = new HttpRequestMessage(HttpMethod.Delete, Address + $"api/v1/privileges/{ticketUid}");
        privilegeRequest.Headers.Add("X-User-Name", username);

        try
        {
            using var privilegeResponse = await client.SendAsync(privilegeRequest);
            if (!privilegeResponse.IsSuccessStatusCode)
                return new(false, (int)privilegeResponse.StatusCode, new ErrorDto(await privilegeResponse.Content.ReadAsStringAsync()));

            return new(true, (int)privilegeResponse.StatusCode);
        }
        catch (Exception ex)
        {
            return new(false, 503, ErrorDto.ServiceUnavailable);
        }
    }
}