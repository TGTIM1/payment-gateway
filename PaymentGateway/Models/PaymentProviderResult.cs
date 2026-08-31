namespace PaymentGateway.Models;

public record PaymentProviderResult
(
    bool IsSuccess,
    string TransactionId,
    string? ErrorMessage
);