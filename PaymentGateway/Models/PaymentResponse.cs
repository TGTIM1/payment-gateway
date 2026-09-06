using PaymentGateway.Models.Enums;

namespace PaymentGateway.Models;

public record PaymentResponse(
    Guid Id,
    decimal Amount,
    string Currency,
    string? Description,
    PaymentStatus Status,
    DateTime CreatedAt,
    string IdempotencyKey,
    long TelegramUserId,
    List<PaymentStatusHistoryDto> StatusHistory
);