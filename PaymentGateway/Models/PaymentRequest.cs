namespace PaymentGateway.Models;

public record PaymentRequest(decimal Amount, string Currency, string IdempotencyKey);