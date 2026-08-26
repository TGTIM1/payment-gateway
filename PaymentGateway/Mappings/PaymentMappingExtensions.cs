using PaymentGateway.Models;

namespace PaymentGateway.Mappings;

public static class PaymentMappingExtensions
{
    public static Payment ToEntity(this PaymentRequest request)
    {
        return new Payment
        {
            Id = new Guid(),
            Amount = request.Amount,
            Currency = request.Currency, 
            Status = PaymentStatus.Created,
            CreatedAt = DateTime.UtcNow,
        };


    }
}