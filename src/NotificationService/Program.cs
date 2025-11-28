using NotificationService.Consumers;
using NotificationService.Services;
using Serilog;

// Serilog Configuration
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Seq(Environment.GetEnvironmentVariable("Seq__ServerUrl") ?? "http://localhost:5341")
    .Enrich.WithProperty("Service", "NotificationService")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// Kafka Consumers - sadece Kafka yapılandırılmışsa ekle
var kafkaBootstrapServers = builder.Configuration["Kafka:BootstrapServers"];
if (!string.IsNullOrEmpty(kafkaBootstrapServers))
{
    try
    {
        Log.Information("Kafka consumer başlatılıyor: {BootstrapServers}", kafkaBootstrapServers);
        builder.Services.AddHostedService<PaymentCompletedConsumer>();
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Kafka consumer başlatılamadı. Servis Kafka olmadan çalışacak.");
    }
}
else
{
    Log.Warning("Kafka yapılandırması bulunamadı. NotificationService Kafka olmadan çalışacak.");
}

// Services
builder.Services.AddSingleton<INotificationService, NotificationServiceImpl>();

// HealthChecks
builder.Services.AddHealthChecks();

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Notification Service API",
        Version = "v1",
        Description = "Notification Service for Evently"
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
