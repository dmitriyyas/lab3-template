using GatewayService.Dto;
using GatewayService.Services;
using System.Threading;

namespace GatewayService;

public enum CircuitBreakerState
{
    Closed,
    Open
}

public class CircuitBreaker(RequestQueue requestQueue)
{
    private readonly RequestQueue _requestQueue = requestQueue;
    private Dictionary<Uri, Func<Task<ServiceResponse<bool>>>> _healthChecks = new();
    private Dictionary<Uri, int> _failures = new();
    private Dictionary<Uri, CircuitBreakerState> _states = new();

    private const int _maxFailures = 3;

    public bool IsOpen(Uri uri) => _states.ContainsKey(uri) && _states[uri] == CircuitBreakerState.Open;

    public async Task<ServiceResponse<T>> ExecuteAsync<T>(Func<Task<ServiceResponse<T>>> action,
        Func<Task<ServiceResponse<bool>>> healthCheck,
        ServiceResponse<T> fallback,
        Uri serviceAddress)
    {
        if (!_failures.ContainsKey(serviceAddress))
        {
            _healthChecks[serviceAddress] = healthCheck;
            _failures[serviceAddress] = 0;
            _states[serviceAddress] = CircuitBreakerState.Closed;
        }

        switch(_states[serviceAddress])
        {
            case CircuitBreakerState.Open:
                return fallback;
            case CircuitBreakerState.Closed:
                try
                {
                    var result = await action();
                    Reset(serviceAddress);
                    return result;
                }
                catch
                {
                    RecordFailure(serviceAddress);
                    return fallback;
                }
            default:
                return fallback;
        }
    }

    private void RecordFailure(Uri address)
    {
        _failures[address]++;
        if (_failures[address] >= _maxFailures)
        {
            Trip(address);
        }
    }

    private void Trip(Uri address)
    {
        Console.WriteLine($"{address} now open");
        _states[address] = CircuitBreakerState.Open;

        _requestQueue.AddRequestToQueue(async () =>
        {
            var response = await _healthChecks[address]();
            if (response.Response)
                Reset(address);

            return response.Response;
        });
    }

    public void Reset(Uri address)
    {
        Console.WriteLine($"{address} now closed");
        _failures[address] = 0;
        _states[address] = CircuitBreakerState.Closed;
    }
}
