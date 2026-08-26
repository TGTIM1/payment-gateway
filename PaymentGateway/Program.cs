using PaymentGateway;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using PaymentGateway.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPaymentEndpoints();

app.Run();