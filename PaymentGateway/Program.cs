using PaymentGateway;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

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

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapGet("/hello", () => Results.Ok(new { status = "Hello World!" }));

app.MapPost("/payments", (PaymentRequest request, AppDbContext db) =>
{
    var payment = new Payment
    {
        Id = Guid.NewGuid(),
        Amount = request.Amount,
        Currency = request.Currency,
        Status = PaymentStatus.Created,
        CreatedAt = DateTime.UtcNow
    };

    db.Payments.Add(payment);
    db.SaveChanges();

    return Results.Created($"/payments/{payment.Id}", payment);
});

app.MapGet("/payments/{id}", (Guid id, AppDbContext db) =>
{
    var payment = db.Payments.FirstOrDefault(x => x.Id == id);
    return payment == null ? Results.NotFound() : Results.Ok(payment);
});

app.MapGet("/payments", (AppDbContext db) => Results.Ok(db.Payments.ToList()));

app.Run();