using EventService.Data;
using EventService.Services;
using Serilog;

// Serilog Configuration
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Seq(Environment.GetEnvironmentVariable("Seq__ServerUrl") ?? "http://localhost:5341")
    .Enrich.WithProperty("Service", "EventService")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// MongoDB Context
builder.Services.AddSingleton<MongoDbContext>();

// Services
builder.Services.AddScoped<IEventService, EventServiceImpl>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<ICalendarService, CalendarService>();
builder.Services.AddScoped<ISearchService, SearchService>();

// HealthChecks
builder.Services.AddHealthChecks();

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Event Service API",
        Version = "v1",
        Description = "Event Management Service for Evently"
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
