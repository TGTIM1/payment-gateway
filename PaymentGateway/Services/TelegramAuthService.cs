using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using PaymentGateway.DTOs;

namespace PaymentGateway.Services;

public class TelegramAuthService : ITelegramAuthService
{
    private readonly string _botToken;

    public TelegramAuthService(IConfiguration configuration)
    {
        _botToken = configuration["TelegramBot:Token"]
            ?? configuration["Telegram:BotToken"]
            ?? throw new ArgumentNullException("Telegram Token not found in configuration");
    }

    public bool ValidateInitData(string initData)
    {
        if (string.IsNullOrWhiteSpace(initData))
            return false;

        string[] pairs = initData.Split('&');
        string? hashPair = pairs.FirstOrDefault(p => p.StartsWith("hash="));

        if (hashPair == null)
            return false;

        string telegramHash = hashPair.Split('=')[1];

   
        string dataCheckString = string.Join("\n", pairs
            .Where(p => !p.StartsWith("hash="))
            .Select(p =>
            {
                var parts = p.Split('=', 2);
                return $"{parts[0]}={Uri.UnescapeDataString(parts[1])}";
            })
            .OrderBy(p => p, StringComparer.Ordinal));

        
        byte[] secretKey;
        using (var hmacToken = new HMACSHA256(Encoding.UTF8.GetBytes("WebAppData")))
        {
            secretKey = hmacToken.ComputeHash(Encoding.UTF8.GetBytes(_botToken));
        }

        byte[] calculatedHashBytes;
        using (var hmacData = new HMACSHA256(secretKey))
        {
            calculatedHashBytes = hmacData.ComputeHash(Encoding.UTF8.GetBytes(dataCheckString));
        }

        string calculatedHash = Convert.ToHexString(calculatedHashBytes).ToLower();

        return string.Equals(calculatedHash, telegramHash, StringComparison.OrdinalIgnoreCase);
    }

    public TelegramUserDto? ParseUserData(string initData)
    {
        if (string.IsNullOrWhiteSpace(initData))
            return null;

        string[] pairs = initData.Split('&');
        string? userPair = pairs.FirstOrDefault(p => p.StartsWith("user="));

        if (userPair == null)
            return null;

        string jsonUser = Uri.UnescapeDataString(userPair.Substring(5));

        try
        {
            return JsonSerializer.Deserialize<TelegramUserDto>(jsonUser);
        }
        catch
        {
            return null;
        }
    }
    public long? ExtractUserId(string? initData)
    {
        if (string.IsNullOrWhiteSpace(initData)) return null;
        if (!ValidateInitData(initData)) return null;
        var userDto = ParseUserData(initData);
        return userDto?.Id;
    }
}