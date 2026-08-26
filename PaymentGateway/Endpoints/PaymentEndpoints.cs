using Microsoft.EntityFrameworkCore;
using PaymentGateway.Mappings;
using PaymentGateway.Models;

namespace PaymentGateway.Endpoints;

public static class PaymentEndpoints
{
    public static void MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/payments");

        group.MapPost("/", (PaymentRequest request, AppDbContext db) =>
        {
            var payment = request.ToEntity();
            db.Payments.Add(payment);
            db.SaveChanges();

            return Results.Created($"/payments/{payment.Id}", payment);
        });
        group.MapGet("/{id:guid}", (Guid id, AppDbContext db) =>
        {
            var payment = db.Payments.FirstOrDefault(x => x.Id == id);
            return payment == null ? Results.NotFound() : Results.Ok(payment);
        });
        group.MapGet("/", (AppDbContext db, int page = 1, int pageSize = 100) =>
        {
            if (page < 1) page = 1;
            if (pageSize > 100) pageSize = 100;

            return Results.Ok(db.Payments
                                .OrderByDescending(x => x.CreatedAt)
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToList());
        });
    }
}