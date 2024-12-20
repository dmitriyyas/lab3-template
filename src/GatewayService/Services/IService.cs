namespace GatewayService.Services;

public interface IService
{
    Task<ServiceResponse<bool>> HealthCheck();

    Uri Address { get; }
}
