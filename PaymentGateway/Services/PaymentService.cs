using Microsoft.EntityFrameworkCore;
using PaymentGateway.Exceptions;
using PaymentGateway.Mappings;
using PaymentGateway.Models;
using PaymentGateway.Models.Enums;

namespace PaymentGateway.Services;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _context;
    private readonly ILogger<PaymentService> _logger;
    private readonly IPaymentProvider _paymentProvider;

    public PaymentService(AppDbContext context, ILogger<PaymentService> logger, IPaymentProvider paymentProvider)
    {
        _context= context;
        _logger = logger;
        _paymentProvider = paymentProvider;
    }
    public async Task<PaymentResponse> CreatePaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default)
    {
        var existingPayment = await _context.Payments.Include(x => x.StatusHistory).FirstOrDefaultAsync(i=>i.IdempotencyKey == request.IdempotencyKey,cancellationToken);
        if (existingPayment != null)
        {
            _logger.LogWarning("Payment with IdempotencyKey {IdempotencyKey} already exists. Returning cached result.", request.IdempotencyKey);
            return existingPayment.ToResponse();
        }
        var payment = request.ToEntity();
        payment.Status = PaymentStatus.Processing;

        payment.StatusHistory.Add(new PaymentStatusHistory
        {
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Processing,
            Reason = "Payment initiated",
            CreatedAt = DateTime.UtcNow,
        });
        
        
        var result = await _paymentProvider.ProcessPaymentAsync(payment.Amount, payment.Currency, cancellationToken);

        if (result.IsSuccess)
        {
            payment.Status = PaymentStatus.Completed;
            payment.StatusHistory.Add(new PaymentStatusHistory
            {
                Id = Guid.NewGuid(),
                Status = PaymentStatus.Completed,
                Reason = $"Processed via PSP. TxId: {result.TransactionId}",
                CreatedAt = DateTime.UtcNow,
            });
        }
        else
        {
            payment.Status = PaymentStatus.Failed;
            payment.StatusHistory.Add(new PaymentStatusHistory
            {
                Id = Guid.NewGuid(),
                Status = PaymentStatus.Failed,
                Reason = result.ErrorMessage ?? "Payment failed",
                CreatedAt = DateTime.UtcNow,
            });
        }
        
        
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);

        return payment.ToResponse();
    }

    public async Task<PaymentResponse> GetPaymentByIdAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        var payment = await _context.Payments
            .AsNoTracking()
            .Include(x=>x.StatusHistory)
            .FirstOrDefaultAsync(x => x.Id == paymentId, cancellationToken);

        if (payment == null)
        {
            throw new NotFoundException($"Payment with id {paymentId} was not found.");
        }
        return payment.ToResponse();
    }

    public async Task<List<PaymentResponse>> GetPaymentAsync(int pages = 1, int pageSize = 100, CancellationToken cancellationToken = default)
    {
     if (pages < 1) pages = 1;
     if (pageSize > 100) pageSize = 100;

     var payments = await _context.Payments
         .AsNoTracking()
         .Include(x => x.StatusHistory)
         .OrderByDescending(x => x.CreatedAt)
         .Skip((pages - 1) * pageSize)
         .Take(pageSize)
         .ToListAsync(cancellationToken);
     return payments.Select(x => x.ToResponse()).ToList();
    }
}

