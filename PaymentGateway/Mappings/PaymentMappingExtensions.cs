using PaymentGateway.Models;
using PaymentGateway.Models.Enums;
namespace PaymentGateway.Mappings;

public static class PaymentMappingExtensions
{
    public static Payment ToEntity(this PaymentRequest request)
    {
        return new Payment
        {
            Id = Guid.NewGuid(),
            Amount = request.Amount,
            Currency = request.Currency, 
            Status = PaymentStatus.Created,
            CreatedAt = DateTime.UtcNow,
            IdempotencyKey = request.IdempotencyKey
        };


    }

    public static PaymentResponse ToResponse(this Payment payment)
    {
        return new PaymentResponse(
            payment.Id,
            payment.Amount,
            payment.Currency,
            payment.Status,
            payment.CreatedAt,
            payment.IdempotencyKey
            );
    }
}