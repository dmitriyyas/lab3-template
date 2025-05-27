using GatewayService.Dto;

namespace GatewayService.Services;

public class StatsService(IHttpClientFactory httpClientFactory,
    CircuitBreaker circuitBreaker) : IService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly CircuitBreaker _circuitBreaker = circuitBreaker;
    private const string _serviceName = "Stats";
    public Uri Address { get; } = new("http://stats_service:8040/");

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

    public async Task<ServiceResponse<StatsDto>> GetStats(string accessToken, int? page = null, int? size = null)
    {
        if (_circuitBreaker.IsOpen(Address))
            return Fallback<StatsDto>();

        var client = _httpClientFactory.CreateClient();
        var query = "api/v1/stats";

        if (page is not null && size is not null)
            query += $"?page={page}&size={size}";

        using var request = new HttpRequestMessage(HttpMethod.Get, Address + query);
        request.Headers.Add("Authorization", accessToken);

        return await _circuitBreaker.ExecuteAsync(async () =>
        {
            using var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return new ServiceResponse<StatsDto>(null, (int)response.StatusCode, new ErrorDto(await response.Content.ReadAsStringAsync()));

            return new ServiceResponse<StatsDto>(await response.Content.ReadFromJsonAsync<StatsDto>());
        }, HealthCheck, Fallback<StatsDto>(), Address);
    }
}
