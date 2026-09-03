using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace PaymentGateway.Services;

public class TelegramBotService : BackgroundService
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<TelegramBotService> _logger;

    public TelegramBotService(IConfiguration configuration, ILogger<TelegramBotService> logger)
    {
        _logger = logger;
        var token = configuration["TelegramBot:Token"] 
                    ?? throw new ArgumentNullException("Telegram Token not found in config");

        _botClient = new TelegramBotClient(token);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var me = await _botClient.GetMe(stoppingToken);
        _logger.LogInformation("Telegram Bot @{BotUsername} started successfully!", me.Username);

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery]
        };

        _botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            errorHandler: HandleErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: stoppingToken
        );
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is { Text: { } messageText } message)
        {

            await HandleMessageAsync(botClient, message, cancellationToken);
        }
    }

    private async Task HandleMessageAsync(ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken)
    {
        string responseText = message.Text;
        switch (responseText)
        {
            case "/start":
                var menuKeyboard = new ReplyKeyboardMarkup(new[]
                {
                   new KeyboardButton[] {"Создать платеж"},
                   new KeyboardButton[] { "История платежей"},
                   new KeyboardButton[] {"Помощь"}
                })
                {
                    ResizeKeyboard = true
                };
                await botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: "Добро пожаловать! Выбери нужный пункт ниже", 
                    replyMarkup:  menuKeyboard,
                    cancellationToken: cancellationToken);
                break;
            case "Создать платеж":
                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    InlineKeyboardButton.WithWebApp(
                        text: "Открыть платежник",
                        webApp: new WebAppInfo{ Url = "https://google.com" })
                });
                await botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: "Нажмите кнопку ниже чтобы создать платеж",
                    replyMarkup: inlineKeyboard,
                    cancellationToken: cancellationToken
                );
                break;
            case "История платежей":
                await botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: "В разработке",
                    cancellationToken: cancellationToken);
                break;
            case "Помощь":
                await botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: "Этот бот предназначен для создания платежей",
                    cancellationToken: cancellationToken);
                break;
            
            default: 
                await botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: "Неизвестная команда, пожалуйста используйте меню",
                    cancellationToken: cancellationToken);
            break;
        }
    }
    private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Error occurred in Telegram Bot");
        return Task.CompletedTask;
    }
}