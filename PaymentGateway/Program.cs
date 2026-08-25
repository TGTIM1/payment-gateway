using PaymentGateway;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.MapGet("/health", () => { return Results.Json(new { status = "ok" }); });
app.MapGet("/hello", () => { return Results.Json(new { status = "Hello World!" }); });
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
app.MapGet("/payments", (AppDbContext db) => { return Results.Ok(db.Payments.ToList); });

app.Run();