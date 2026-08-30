using PaymentGateway.Models.Enums;

namespace PaymentGateway.Models;

public record PaymentStatusHistoryDto
(
    PaymentStatus Status,
    string? Reason,
    DateTime CreatedAt
);