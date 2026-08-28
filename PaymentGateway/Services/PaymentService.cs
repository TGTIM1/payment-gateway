using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Mappings;
using PaymentGateway.Models;

namespace PaymentGateway.Services;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _context;

    public PaymentService(AppDbContext context)
    {
        _context= context;
    }
    public async Task<PaymentResponse> CreatePaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default)
    {
        var payment = request.ToEntity();
        _context.Payments.Add(payment);

        await _context.SaveChangesAsync(cancellationToken);

        return payment.ToResponse();
    }

    public async Task<PaymentResponse?> GetPaymentByIdAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        var payment = await _context.Payments.FirstOrDefaultAsync(x => x.Id == paymentId, cancellationToken);

        return payment == null ? null : payment.ToResponse();

    }

    public async Task<List<PaymentResponse>> GetPaymentAsync(int pages = 1, int pageSize = 100, CancellationToken cancellationToken = default)
    {
     if (pages < 1) pages = 1;
     if (pageSize > 100) pageSize = 100;

     var payments = await _context.Payments
         .AsNoTracking()
         .OrderByDescending(x => x.CreatedAt)
         .Skip((pages - 1) * pageSize)
         .Take(pageSize)
         .ToListAsync(cancellationToken);
     return payments.Select(x => x.ToResponse()).ToList();
    }
}

