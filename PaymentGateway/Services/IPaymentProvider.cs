using PaymentGateway.Models;

namespace PaymentGateway.Services;

public interface IPaymentProvider
{
    Task<PaymentProviderResult> ProcessPaymentAsync(decimal amount, string currency, CancellationToken cancellationToken = default);
}