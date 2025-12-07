using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Victorina.Application.Interfaces;
using Victorina.Application.Services;
using Victorina.Domain.Enums;

namespace Victorina.Bot.Services;

/// <summary>
/// Service that monitors game sessions and sends notifications to players when bot opponent joins
/// </summary>
public class BotMatchmakingNotificationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BotMatchmakingNotificationService> _logger;
    private readonly ITelegramBotClient _bot;
    private readonly KeyboardService _keyboard;
    private readonly HashSet<int> _notifiedGames = new();
    private const int CheckIntervalMs = 500; // Check every 500ms

    public BotMatchmakingNotificationService(
        IServiceProvider serviceProvider,
        ILogger<BotMatchmakingNotificationService> logger,
        ITelegramBotClient bot,
        KeyboardService keyboard)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _bot = bot;
        _keyboard = keyboard;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BotMatchmakingNotificationService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndNotifyPlayersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BotMatchmakingNotificationService");
            }

            await Task.Delay(CheckIntervalMs, stoppingToken);
        }

        _logger.LogInformation("BotMatchmakingNotificationService stopped");
    }

    private async Task CheckAndNotifyPlayersAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var sessionStore = scope.ServiceProvider.GetRequiredService<GameSessionStore>();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var questionService = scope.ServiceProvider.GetRequiredService<IQuestionService>();

        // Get all sessions (both active and waiting for ready)
        var allSessions = sessionStore.GetAllSessions().ToList();
        var waitingForReadySessions = allSessions
            .Where(s => s.Status == GameStatus.WaitingForReady && s.Players.Count == 2)
            .ToList();

        foreach (var session in waitingForReadySessions)
        {
            // Skip if already notified
            if (_notifiedGames.Contains(session.GameId))
                continue;

            // Check if one of the players is a bot
            var playersList = session.Players.Values.ToList();
            var user1 = await userService.GetByTelegramIdAsync(playersList[0].TelegramId);
            var user2 = await userService.GetByTelegramIdAsync(playersList[1].TelegramId);

            if (user1 == null || user2 == null)
                continue;

            // If one is a bot, send notification to the human player
            if (user1.IsBot || user2.IsBot)
            {
                var humanPlayer = user1.IsBot ? playersList[1] : playersList[0];
                var botPlayer = user1.IsBot ? playersList[0] : playersList[1];
                var botUser = user1.IsBot ? user1 : user2;

                try
                {
                    var lang = humanPlayer.LanguageCode;
                    var botName = botPlayer.FirstName ?? "Bot";
                    var botFlag = GetCountryFlag(botPlayer.CountryCode);

                    // Get category info
                    var categoryInfo = "";
                    if (session.CategoryTranslationGroupId != null || session.CategoryId != null)
                    {
                        var categoryName = await questionService.GetCategoryNameAsync(
                            session.CategoryTranslationGroupId, session.CategoryId, lang);
                        if (categoryName != null)
                        {
                            categoryInfo = LocalizationService.Get(lang, "category_info", categoryName);
                        }
                    }

                    await _bot.SendMessage(humanPlayer.TelegramId,
                        $"{LocalizationService.Get(lang, "opponent_found")}\n\n{botFlag} *{botName}*{categoryInfo}\n\n{LocalizationService.Get(lang, "btn_ready")}!",
                        parseMode: ParseMode.Markdown,
                        replyMarkup: _keyboard.GetReadyKeyboard(lang));

                    _logger.LogInformation(
                        "Sent bot matchmaking notification to player {PlayerId} for game {GameId}",
                        humanPlayer.TelegramId, session.GameId);

                    // Mark as notified
                    _notifiedGames.Add(session.GameId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to send bot matchmaking notification to player {PlayerId}",
                        humanPlayer.TelegramId);
                }
            }
        }

        // Clean up old notified games (games that are no longer waiting for ready)
        var activeGameIds = waitingForReadySessions.Select(s => s.GameId).ToHashSet();
        _notifiedGames.RemoveWhere(id => !activeGameIds.Contains(id));
    }

    private string GetCountryFlag(string? countryCode)
    {
        if (string.IsNullOrEmpty(countryCode) || countryCode.Length != 2)
            return "🌍";

        countryCode = countryCode.ToUpper();
        var flag = string.Concat(countryCode.Select(c => char.ConvertFromUtf32(c + 0x1F1A5)));
        return flag;
    }
}
