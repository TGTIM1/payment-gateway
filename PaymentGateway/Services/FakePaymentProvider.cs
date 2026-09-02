using PaymentGateway.Models;

namespace PaymentGateway.Services;

public class FakePaymentProvider : IPaymentProvider
{
    public async Task<PaymentProviderResult> ProcessPaymentAsync(decimal amount, string currency,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(500);

        int roll = Random.Shared.Next(1, 101);

        if (roll <= 30)
        {
            return new PaymentProviderResult
            (
                IsSuccess: true,
                TransactionId: $"psp_tx_{Guid.NewGuid():N}",
                ErrorMessage: null
            );

        }
        else
        {
            return new PaymentProviderResult
            (
                IsSuccess: false,
                TransactionId: string.Empty,
                ErrorMessage: "Card declined or insufficient funds"
            );
        }
    }

    
}