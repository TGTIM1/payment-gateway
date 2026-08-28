using PaymentGateway.Models;

namespace PaymentGateway.Services;

public interface IPaymentService
{
    Task<PaymentResponse> CreatePaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default);
    Task<PaymentResponse?> GetPaymentByIdAsync(Guid paymentId, CancellationToken cancellationToken = default);
    Task<List<PaymentResponse>> GetPaymentAsync(int pages, int pageSize, CancellationToken cancellationToken = default);
}