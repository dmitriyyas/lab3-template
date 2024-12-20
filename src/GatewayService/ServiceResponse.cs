using GatewayService.Dto;

namespace GatewayService;

public class ServiceResponse<T>(T? response, int statusCode = 200, ErrorDto? errorDto = null)
{
    public T? Response { get; set; } = response;
    public int StatusCode { get; set; } = statusCode;
    public ErrorDto? ErrorDto { get; set; } = errorDto;

    public static ServiceResponse<T> Fallback(string name) => new(default(T?), 503, ErrorDto.ServiceUnavailable(name));
}
