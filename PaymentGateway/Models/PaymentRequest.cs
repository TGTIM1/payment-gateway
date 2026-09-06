namespace PaymentGateway.Models;

public record PaymentRequest(decimal Amount, string Currency,string Description, string IdempotencyKey, long TelegramUserId);