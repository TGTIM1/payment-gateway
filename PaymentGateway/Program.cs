using PaymentGateway;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var payments = new List<Payment>();
app.MapGet("/health", () => { return Results.Json(new { status = "ok" }); });
app.MapGet("/hello", () => { return Results.Json(new { status = "Hello World!" }); });
app.MapPost("/payments", (Payment payment) =>
{
    payment.Id = Guid.NewGuid();
    payment.CreatedAt = DateTime.UtcNow;
    payments.Add(payment);
    return Results.Created($"/payments/{payment.Id}", payment);
});
app.MapGet("/payments/{id}", (Guid id) =>
{
    var payment = payments.FirstOrDefault(x => x.Id == id);
    if (payment == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(payment);
});
app.MapGet("/payments", () => { return Results.Ok(payments); });

app.Run();