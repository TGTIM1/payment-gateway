using PaymentGateway.Models;
using FluentValidation;
using PaymentGateway.Services;

namespace PaymentGateway.Endpoints;

public static class PaymentEndpoints
{
    public static void MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/payments");

        group.MapPost("/", async (PaymentRequest request, IPaymentService paymentService, IValidator<PaymentRequest> validator, CancellationToken cancellationToken) =>
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }
            var response = await paymentService.CreatePaymentAsync(request, cancellationToken);
            return Results.Created($"/payments/{response.Id}", response);
            
        });
        group.MapGet("/{id:guid}",async (Guid id, IPaymentService paymentService, CancellationToken cancellationToken) =>
        {
            var response  = await paymentService.GetPaymentByIdAsync(id, cancellationToken);
            return Results.Ok(response);
        });
        group.MapGet("/",async (IPaymentService paymentService, CancellationToken cancellationToken, int page = 1, int pageSize = 10) =>
        {
            var response = await paymentService.GetPaymentAsync(page, pageSize, cancellationToken);
            return Results.Ok(response);
        });
    }
}