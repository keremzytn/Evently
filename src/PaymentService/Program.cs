using PaymentService.Consumers;
using PaymentService.Services;
using Serilog;
using Shared.Kafka;

// Serilog Configuration
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Seq(Environment.GetEnvironmentVariable("Seq__ServerUrl") ?? "http://localhost:5341")
    .Enrich.WithProperty("Service", "PaymentService")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// Kafka - opsiyonel
var kafkaBootstrapServers = builder.Configuration["Kafka:BootstrapServers"];
if (!string.IsNullOrEmpty(kafkaBootstrapServers))
{
    Log.Information("Kafka producer başlatılıyor: {BootstrapServers}", kafkaBootstrapServers);
    builder.Services.AddSingleton(new KafkaProducer(kafkaBootstrapServers));

    // Kafka Consumers
    builder.Services.AddHostedService<TicketCreatedConsumer>();
}
else
{
    Log.Warning("Kafka yapılandırması bulunamadı. PaymentService Kafka olmadan çalışacak.");
    builder.Services.AddSingleton<KafkaProducer>(_ => null!);
}

// Services
builder.Services.AddScoped<IPaymentService, PaymentServiceImpl>();
builder.Services.AddSingleton<IPromoCodeService, PromoCodeService>();

// HealthChecks
builder.Services.AddHealthChecks();

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Payment Service API",
        Version = "v1",
        Description = "Payment Processing Service for Evently"
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
