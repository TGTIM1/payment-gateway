namespace PaymentGateway.Models.Enums;
using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Currency
{
    USD,
    UZS,
    EUR,
    RUB,
}