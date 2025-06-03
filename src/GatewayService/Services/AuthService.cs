using GatewayService.Dto;
using Microsoft.AspNetCore.Mvc;

namespace GatewayService.Services;

public class AuthService(IHttpClientFactory httpClientFactory,
    CircuitBreaker circuitBreaker) : IService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly CircuitBreaker _circuitBreaker = circuitBreaker;
    private const string _serviceName = "Auth";
    public Uri Address { get; } = new("http://services-auth-srv:8090/");

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

    public async Task<ServiceResponse<string>> Login(AuthDataDto authData)
    {
        if (_circuitBreaker.IsOpen(Address))
            return Fallback<string>();

        var client = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, Address + $"api/v1/login");
        request.Content = JsonContent.Create(authData);

        try
        {
            using var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return new(null, (int)response.StatusCode, new ErrorDto(await response.Content.ReadAsStringAsync()));
            return new(await response.Content.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            return Fallback<string>();
        }
    }

    public async Task<ServiceResponse<string>> GetToken(TokenRequestDto tokenRequest)
    {
        if (_circuitBreaker.IsOpen(Address))
            return Fallback<string>();

        var client = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, Address + $"api/v1/token");
        request.Content = JsonContent.Create(tokenRequest);

        try
        {
            using var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return new(null, (int)response.StatusCode, new ErrorDto(await response.Content.ReadAsStringAsync()));
            return new(await response.Content.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            return Fallback<string>();
        }
    }

    public async Task<ServiceResponse<bool>> Register(UserCreateDto userCreate)
    {
        if (_circuitBreaker.IsOpen(Address))
            return Fallback<bool>();

        var client = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, Address + $"api/v1/users");
        request.Content = JsonContent.Create(userCreate);

        try
        {
            using var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return new(false, (int)response.StatusCode, new ErrorDto(await response.Content.ReadAsStringAsync()));
            return new(true);
        }
        catch (Exception ex)
        {
            return Fallback<bool>();
        }
    }

    public async Task<ServiceResponse<UserDto>> GetUserInfo(string accessToken)
    {
        if (_circuitBreaker.IsOpen(Address))
            return Fallback<UserDto>();

        var client = _httpClientFactory.CreateClient();
        using var userRequest = new HttpRequestMessage(HttpMethod.Get, Address + "api/v1/me");
        userRequest.Headers.Add("Authorization", accessToken);

        return await _circuitBreaker.ExecuteAsync(async () =>
        {
            using var response = await client.SendAsync(userRequest);
            if (!response.IsSuccessStatusCode)
                return new ServiceResponse<UserDto>(null, (int)response.StatusCode, new ErrorDto(await response.Content.ReadAsStringAsync()));

            return new ServiceResponse<UserDto>(await response.Content.ReadFromJsonAsync<UserDto>());
        }, HealthCheck, Fallback<UserDto>(), Address);
    }
}
