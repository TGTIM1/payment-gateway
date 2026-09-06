using PaymentGateway.DTOs;

namespace PaymentGateway.Services;

public interface ITelegramAuthService
{
    bool ValidateInitData(string initData);
    TelegramUserDto? ParseUserData(string initData);
    long? ExtractUserId(string? initData);
}