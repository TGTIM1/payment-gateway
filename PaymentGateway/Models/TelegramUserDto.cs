using System.Text.Json.Serialization;

namespace PaymentGateway.DTOs;

public class TelegramUserDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("language_code")]
    public string? LanguageCode { get; set; }
}