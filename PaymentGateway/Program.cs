var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/health", () =>
{
    return Results.Json(new {status = "ok"});
});
app.MapGet("/hello", () =>
{
    return Results.Json(new { status = "Hello World" }); 
});
app.Run();

