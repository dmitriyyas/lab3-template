using Common;
using Confluent.Kafka;
using GatewayService;
using GatewayService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGenWithJwt();
builder.Services.AddOauth();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<BonusService>();
builder.Services.AddSingleton<FlightService>();
builder.Services.AddSingleton<TicketService>();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<StatsService>();
builder.Services.AddSingleton<CircuitBreaker>();
builder.Services.AddSingleton<RequestQueue>();

builder.Services.AddSingleton<IProducer<Null, string>>(sp =>
{
    var config = new ProducerConfig
    {
        BootstrapServers = "kafka:9092"
    };
    return new ProducerBuilder<Null, string>(config).Build();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseCors(builder =>
{
    builder.AllowAnyOrigin();
    builder.AllowAnyHeader();
    builder.AllowAnyMethod();
});

app.UseMiddleware<StatsMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
