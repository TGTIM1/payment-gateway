using PaymentGateway.Models;
using PaymentGateway.Models.Enums;
using Microsoft.EntityFrameworkCore;
namespace PaymentGateway.Services;



public class PaymentProcessingWorker : BackgroundService
{
    private readonly ILogger<PaymentProcessingWorker> _logger;
    private readonly IServiceProvider  _serviceProvider;
    
    public PaymentProcessingWorker(
        ILogger<PaymentProcessingWorker> logger,
        IServiceProvider serviceProvider){
        _logger = logger;
        _serviceProvider = serviceProvider;}

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"Payment processing started at {DateTime.Now}");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingPaymentsAsync(stoppingToken);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"Error occured at processing payment");
            }
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task ProcessPendingPaymentsAsync(CancellationToken stoppingToken)
{

    List<Guid> pendingIds;
    using (var scope = _serviceProvider.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        pendingIds = await dbContext.Payments
            .Where(p => p.Status == PaymentStatus.Pending)
            .OrderBy(p => p.CreatedAt)
            .Select(p => p.Id)
            .Take(10)
            .ToListAsync(stoppingToken);
    }

    if (!pendingIds.Any()) return;

    _logger.LogInformation("Found {Count} pending payments for processing", pendingIds.Count);


    foreach (var paymentId in pendingIds)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var paymentProvider = scope.ServiceProvider.GetRequiredService<IPaymentProvider>();

        var payment = await dbContext.Payments.FirstOrDefaultAsync(p => p.Id == paymentId, stoppingToken);
        if (payment == null || payment.Status != PaymentStatus.Pending) continue;


        payment.Status = PaymentStatus.Processing;
        dbContext.StatusHistory.Add(new PaymentStatusHistory
        {
            Id = Guid.NewGuid(),
            PaymentId = payment.Id,
            Status = PaymentStatus.Processing,
            Reason = $"Processing started by background worker (Attempt {payment.RetryCount + 1})",
            CreatedAt = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync(stoppingToken);


        var result = await paymentProvider.ProcessPaymentAsync(payment.Amount, payment.Currency, stoppingToken);

 
        if (result.IsSuccess)
        {
            payment.Status = PaymentStatus.Completed;
            dbContext.StatusHistory.Add(new PaymentStatusHistory
            {
                Id = Guid.NewGuid(),
                PaymentId = payment.Id,
                Status = PaymentStatus.Completed,
                Reason = $"Processed via PSP. TxId: {result.TransactionId}",
                CreatedAt = DateTime.UtcNow
            });
            _logger.LogInformation("Payment {PaymentId} processed successfully", payment.Id);
        }
        else
        {
            payment.RetryCount++;

            if (payment.RetryCount < 10)
            {
                payment.Status = PaymentStatus.Pending;
                dbContext.StatusHistory.Add(new PaymentStatusHistory
                {
                    Id = Guid.NewGuid(),
                    PaymentId = payment.Id,
                    Status = PaymentStatus.Pending,
                    Reason = $"PSP failed: {result.ErrorMessage}. Retrying ({payment.RetryCount}/3)...",
                    CreatedAt = DateTime.UtcNow
                });
                _logger.LogWarning("Payment {PaymentId} failed. Retry {RetryCount}/3 scheduled", payment.Id, payment.RetryCount);
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
                dbContext.StatusHistory.Add(new PaymentStatusHistory
                {
                    Id = Guid.NewGuid(),
                    PaymentId = payment.Id,
                    Status = PaymentStatus.Failed,
                    Reason = $"Payment failed after {payment.RetryCount} retries. Error: {result.ErrorMessage}",
                    CreatedAt = DateTime.UtcNow
                });
                _logger.LogWarning("Payment {PaymentId} permanently failed after {RetryCount} retries", payment.Id, payment.RetryCount);
            }

            
        }

        await dbContext.SaveChangesAsync(stoppingToken);
    }
}
}