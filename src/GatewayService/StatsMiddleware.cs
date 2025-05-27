using Confluent.Kafka;
using System.Diagnostics;
using System.Text.Json;

namespace GatewayService;

public class StatsMiddleware(RequestDelegate next, IProducer<Null, string> kafkaProducer)
{
    private readonly RequestDelegate _next = next;
    private readonly IProducer<Null, string> _kafkaProducer = kafkaProducer;
    private const string Topic = "request-stats";

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        var requestMethod = context.Request.Method;
        var requestPath = context.Request.Path;

        await _next(context);

        stopwatch.Stop();

        var statusCode = context.Response.StatusCode;

        var stats = new
        {
            Method = requestMethod,
            Path = requestPath.Value,
            StatusCode = statusCode,
            DurationMs = stopwatch.ElapsedMilliseconds,
            Timestamp = DateTime.UtcNow
        };

        var message = new Message<Null, string> { Value = JsonSerializer.Serialize(stats) };
        await _kafkaProducer.ProduceAsync(Topic, message);
    }
}

