using PaymentGateway;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using FluentValidation;
using PaymentGateway.Endpoints;
using PaymentGateway.Middleware;
using PaymentGateway.Models;
using PaymentGateway.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPaymentProvider, FakePaymentProvider>();
builder.Services.AddScoped<ITelegramAuthService, TelegramAuthService>();
builder.Services.AddHostedService<PaymentProcessingWorker>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173") 
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.AddHostedService<TelegramBotService>();

// Настройка сериализации Enum в строку для всех типов ответов (и Http.Json, и Mvc.Json)
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

// Автоматическое применение миграций при старте приложения
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseExceptionHandler();
app.MapPaymentEndpoints();
app.MapAuthEndpoints();
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}
app.Run();