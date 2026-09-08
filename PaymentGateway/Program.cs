using PaymentGateway;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using FluentValidation;
using PaymentGateway.Endpoints;
using PaymentGateway.Middleware;
using PaymentGateway.Services;
using Serilog;

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .Build())
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/payment-gateway-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Запуск веб-хоста PaymentGateway...");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // 2. Регистрация сервисов
    builder.Services.AddOpenApi();
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    builder.Services.AddScoped<IPaymentService, PaymentService>();
    builder.Services.AddScoped<IPaymentProvider, FakePaymentProvider>();
    builder.Services.AddScoped<ITelegramAuthService, TelegramAuthService>();

    builder.Services.AddHostedService<PaymentProcessingWorker>();
    builder.Services.AddHostedService<TelegramBotService>();

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

    // Настройка JSON
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

    // Pipeline 
    app.UseMiddleware<CorrelationIdMiddleware>(); 
    app.UseSerilogRequestLogging();                

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

    //  Миграции базы данных перед стартом
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Приложение завершилось с критической ошибкой!");
}
finally
{
    Log.CloseAndFlush();
}