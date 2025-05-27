using Confluent.Kafka;
using StatsService.Models;
using System.Text.Json;

namespace StatsService;

public class StatsConsumerService(IServiceScopeFactory scopeFactory, ILogger<StatsConsumerService> logger) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<StatsConsumerService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = "kafka:9092",
            GroupId = "stats-consumer",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe("request-stats");

        _logger.LogInformation("StatsConsumerService started.");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var cr = consumer.Consume(stoppingToken);
                    var stat = JsonSerializer.Deserialize<Stat>(cr.Message.Value);

                    using var scope = _scopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<StatsContext>();

                    context.Stats.Add(stat);
                    await context.SaveChangesAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // graceful shutdown
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while consuming message");
                }
            }
        }
        finally
        {
            consumer.Close();
        }

        _logger.LogInformation("StatsConsumerService stopped.");
    }
}
