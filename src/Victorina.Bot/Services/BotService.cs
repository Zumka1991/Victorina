using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Victorina.Bot.Handlers;

namespace Victorina.Bot.Services;

public class BotService : BackgroundService
{
    private readonly ITelegramBotClient _bot;
    private readonly UpdateHandler _handler;
    private readonly ILogger<BotService> _logger;

    public BotService(
        ITelegramBotClient bot,
        UpdateHandler handler,
        ILogger<BotService> logger)
    {
        _bot = bot;
        _handler = handler;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var me = await _bot.GetMe(stoppingToken);
        _logger.LogInformation("Bot {Username} started", me.Username);

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = [],
            DropPendingUpdates = true
        };

        await _bot.ReceiveAsync(
            updateHandler: (bot, update, ct) => _handler.HandleUpdateAsync(update, ct),
            errorHandler: (bot, exception, ct) => HandleErrorAsync(exception),
            receiverOptions: receiverOptions,
            cancellationToken: stoppingToken);
    }

    private Task HandleErrorAsync(Exception exception)
    {
        _logger.LogError(exception, "Telegram bot error");
        return Task.CompletedTask;
    }
}
