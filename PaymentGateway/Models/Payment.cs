using System.Text.Json.Serialization;

namespace PaymentGateway.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentStatus
{
    Created,
    Processing,
    Completed,
    Failed,
    Cancelled
}

public class Payment
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public PaymentStatus Status { get; set; } = PaymentStatus.Created;
    public DateTime CreatedAt { get; set; }
}