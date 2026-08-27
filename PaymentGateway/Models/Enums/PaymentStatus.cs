namespace PaymentGateway.Models.Enums;
using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentStatus
{
    Created,
    Processing,
    Completed,
    Failed,
    Cancelled
}