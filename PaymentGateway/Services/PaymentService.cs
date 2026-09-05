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

    public PaymentService(AppDbContext context, ILogger<PaymentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaymentResponse> CreatePaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default)
    {
        var existingPayment = await _context.Payments
            .Include(x => x.StatusHistory)
            .FirstOrDefaultAsync(i => i.IdempotencyKey == request.IdempotencyKey, cancellationToken);

        if (existingPayment != null)
        {
            _logger.LogWarning("Payment with IdempotencyKey {IdempotencyKey} already exists. Returning cached result.", request.IdempotencyKey);
            return existingPayment.ToResponse();
        }

        var payment = request.ToEntity();
        payment.Status = PaymentStatus.Pending;

        payment.StatusHistory.Add(new PaymentStatusHistory
        {
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Pending,
            Reason = "Payment created and queued for processing",
            CreatedAt = DateTime.UtcNow,
        });

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);

        return payment.ToResponse();
    }

    public async Task<PaymentResponse> GetPaymentByIdAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        var payment = await _context.Payments
            .AsNoTracking()
            .Include(x => x.StatusHistory)
            .FirstOrDefaultAsync(x => x.Id == paymentId, cancellationToken);

        if (payment == null)
        {
            throw new NotFoundException($"Payment with id {paymentId} was not found.");
        }
        return payment.ToResponse();
    }

    
    public async Task<List<PaymentResponse>> GetPaymentAsync(long telegramUserId, int pages = 1, int pageSize = 100, CancellationToken cancellationToken = default)
    {
        if (pages < 1) pages = 1;
        if (pageSize > 100) pageSize = 100;

        var payments = await _context.Payments
            .AsNoTracking()
            .Include(x => x.StatusHistory)
            .Where(x => x.TelegramUserId == telegramUserId)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pages - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return payments.Select(x => x.ToResponse()).ToList();
    }
}