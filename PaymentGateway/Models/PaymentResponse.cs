using PaymentGateway.Models.Enums;

namespace PaymentGateway.Models;

public record PaymentResponse(
    Guid Id,
    decimal Amount,
    string Currency,
    PaymentStatus Status,
    DateTime CreatedAt
);