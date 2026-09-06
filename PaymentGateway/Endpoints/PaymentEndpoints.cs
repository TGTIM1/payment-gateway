using PaymentGateway.Models;
using FluentValidation;
using PaymentGateway.Services;

namespace PaymentGateway.Endpoints;

public static class PaymentEndpoints
{
    public static void MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/payments");

        group.MapPost("/", async (
            PaymentRequest request, 
            IPaymentService paymentService, 
            ITelegramAuthService authService,
            IValidator<PaymentRequest> validator, 
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var initData = httpContext.Request.Headers["X-Telegram-Init-Data"].ToString();
            var realUserId = authService.ExtractUserId(initData);

            if (realUserId.HasValue)
            {
                request = request with { TelegramUserId = realUserId.Value };
            }

            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var response = await paymentService.CreatePaymentAsync(request, cancellationToken);
            return Results.Created($"/payments/{response.Id}", response);
        });

        group.MapGet("/{id:guid}", async (Guid id, IPaymentService paymentService, CancellationToken cancellationToken) =>
        {
            var response = await paymentService.GetPaymentByIdAsync(id, cancellationToken);
            return Results.Ok(response);
        });
        
        group.MapGet("/", async (
            IPaymentService paymentService, 
            ITelegramAuthService authService,
            HttpContext httpContext,
            CancellationToken cancellationToken, 
            long? telegramUserId = null, 
            int page = 1, 
            int pageSize = 10) =>
        {
            var initData = httpContext.Request.Headers["X-Telegram-Init-Data"].ToString();
            var realUserId = authService.ExtractUserId(initData);

            long targetUserId = realUserId ?? telegramUserId ?? 100;

            var response = await paymentService.GetPaymentAsync(targetUserId, page, pageSize, cancellationToken);
            return Results.Ok(response);
        });
    }
}