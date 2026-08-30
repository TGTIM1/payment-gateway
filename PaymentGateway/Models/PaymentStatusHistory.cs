using PaymentGateway.Models.Enums;

namespace PaymentGateway.Models;

public class PaymentStatusHistory
{
    public Guid Id { get; set; }
    public Guid PaymentId { get; set; }
    public PaymentStatus Status { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Payment? Payment { get; set; }
}