
using PaymentGateway.Models.Enums;
namespace PaymentGateway.Models;


public class Payment
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; init; } = "USD";
    public PaymentStatus Status { get; set; } = PaymentStatus.Created;
    public DateTime CreatedAt { get; set; }

    public string IdempotencyKey { get; set; } = string.Empty;

    public List<PaymentStatusHistory> StatusHistory { get; set; } = new();
}