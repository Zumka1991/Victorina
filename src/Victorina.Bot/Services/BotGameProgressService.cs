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
/// Service that monitors games with bots and sends next questions/results to human players
/// </summary>
public class BotGameProgressService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BotGameProgressService> _logger;
    private readonly ITelegramBotClient _bot;
    private readonly KeyboardService _keyboard;
    private readonly HashSet<string> _processedTransitions = new(); // Format: "gameId:questionIndex"
    private const int CheckIntervalMs = 500; // Check every 500ms

    public BotGameProgressService(
        IServiceProvider serviceProvider,
        ILogger<BotGameProgressService> logger,
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
        _logger.LogInformation("BotGameProgressService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckGameProgressAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BotGameProgressService");
            }

            await Task.Delay(CheckIntervalMs, stoppingToken);
        }

        _logger.LogInformation("BotGameProgressService stopped");
    }

    private async Task CheckGameProgressAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var sessionStore = scope.ServiceProvider.GetRequiredService<GameSessionStore>();
        var gameService = scope.ServiceProvider.GetRequiredService<IGameService>();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        // Get all active game sessions
        var activeSessions = sessionStore.GetActiveGameSessions().ToList();

        foreach (var session in activeSessions)
        {
            // Only process games with 2 players
            if (session.Players.Count != 2)
                continue;

            // Check if this is a game with a bot
            var players = session.Players.Values.ToList();
            var user1 = await userService.GetByTelegramIdAsync(players[0].TelegramId);
            var user2 = await userService.GetByTelegramIdAsync(players[1].TelegramId);

            if (user1 == null || user2 == null)
                continue;

            // Skip if no bots in this game
            if (!user1.IsBot && !user2.IsBot)
                continue;

            var humanPlayer = user1.IsBot ? players[1] : players[0];
            var botPlayer = user1.IsBot ? players[0] : players[1];

            // Check if both players have answered the current question
            // Both players should have moved to the next question (CurrentQuestionIndex > session.CurrentQuestionIndex)
            bool bothAnswered = humanPlayer.CurrentQuestionIndex > session.CurrentQuestionIndex &&
                               botPlayer.CurrentQuestionIndex > session.CurrentQuestionIndex;

            if (bothAnswered)
            {
                var transitionKey = $"{session.GameId}:{session.CurrentQuestionIndex}";

                // Skip if already processed this transition
                if (_processedTransitions.Contains(transitionKey))
                    continue;

                _logger.LogInformation(
                    "Both players answered question {QuestionIndex} in game {GameId}",
                    session.CurrentQuestionIndex, session.GameId);

                // Mark as processed
                _processedTransitions.Add(transitionKey);

                // Check if this was the last question
                if (session.CurrentQuestionIndex + 1 >= session.Questions.Count)
                {
                    // Game is over - send results
                    var gameResult = await gameService.FinishGameAsync(session.GameId);
                    if (gameResult != null)
                    {
                        await SendGameResultsToHumanAsync(gameResult, userService);
                    }
                }
                else
                {
                    // Move to next question
                    await gameService.MoveToNextQuestionAsync(session.GameId);
                    var updatedSession = await gameService.GetGameByIdAsync(session.GameId);

                    if (updatedSession != null)
                    {
                        // Get question in player's language
                        var playerQuestion = humanPlayer.Questions.Count > updatedSession.CurrentQuestionIndex
                            ? humanPlayer.Questions[updatedSession.CurrentQuestionIndex]
                            : updatedSession.Questions[updatedSession.CurrentQuestionIndex];

                        var lang = await GetUserLanguageAsync(humanPlayer.TelegramId, userService);

                        await Task.Delay(1500);
                        await SendQuestionAsync(humanPlayer.TelegramId, lang, playerQuestion,
                            updatedSession.CurrentQuestionIndex + 1, updatedSession.Questions.Count);
                    }
                }
            }
        }

        // Clean up old processed transitions
        var activeGameIds = activeSessions.Select(s => s.GameId).ToHashSet();
        _processedTransitions.RemoveWhere(key =>
        {
            var gameId = int.Parse(key.Split(':')[0]);
            return !activeGameIds.Contains(gameId);
        });
    }

    private async Task<string> GetUserLanguageAsync(long telegramId, IUserService userService)
    {
        var user = await userService.GetByTelegramIdAsync(telegramId);
        return user?.LanguageCode ?? "ru";
    }

    private async Task SendQuestionAsync(long chatId, string lang, Application.Models.GameSessionQuestion question,
        int currentNumber, int totalQuestions)
    {
        var questionText = $"*{LocalizationService.Get(lang, "question_label")} {currentNumber}/{totalQuestions}*\n\n{question.Text}";

        if (!string.IsNullOrEmpty(question.ImageUrl))
        {
            await _bot.SendPhoto(
                chatId,
                Telegram.Bot.Types.InputFile.FromUri(question.ImageUrl),
                caption: questionText,
                parseMode: ParseMode.Markdown,
                replyMarkup: _keyboard.GetQuestionKeyboard(question.Answers.ToArray()));
        }
        else
        {
            await _bot.SendMessage(
                chatId,
                questionText,
                parseMode: ParseMode.Markdown,
                replyMarkup: _keyboard.GetQuestionKeyboard(question.Answers.ToArray()));
        }
    }

    private async Task SendGameResultsToHumanAsync(Application.Models.GameResult result, IUserService userService)
    {
        // Find the human player
        var user1 = await userService.GetByTelegramIdAsync(result.Player1.TelegramId);
        var user2 = await userService.GetByTelegramIdAsync(result.Player2.TelegramId);

        Application.Models.PlayerResult? humanResult = null;
        Application.Models.PlayerResult? botResult = null;

        if (user1 != null && !user1.IsBot)
        {
            humanResult = result.Player1;
            botResult = result.Player2;
        }
        else if (user2 != null && !user2.IsBot)
        {
            humanResult = result.Player2;
            botResult = result.Player1;
        }

        if (humanResult == null || botResult == null)
            return;

        var lang = await GetUserLanguageAsync(humanResult.TelegramId, userService);
        var isWinner = result.WinnerTelegramId == humanResult.TelegramId;

        string title;
        if (result.IsDraw)
        {
            title = LocalizationService.Get(lang, "draw");
        }
        else if (isWinner)
        {
            title = LocalizationService.Get(lang, "you_won");
        }
        else
        {
            title = LocalizationService.Get(lang, "you_lost");
        }

        var opponentFlag = CountryService.GetFlag(botResult.CountryCode);
        var opponentName = botResult.GetDisplayName();

        var message = $"{title}\n\n" +
                     LocalizationService.Get(lang, "your_result") + $"\n" +
                     LocalizationService.Get(lang, "correct_answers", humanResult.CorrectAnswers) + $"\n" +
                     LocalizationService.Get(lang, "time_spent", humanResult.TotalTime.TotalSeconds.ToString("F2")) + $"\n\n" +
                     LocalizationService.Get(lang, "opponent_result", opponentFlag, opponentName) + $"\n" +
                     LocalizationService.Get(lang, "correct_answers", botResult.CorrectAnswers) + $"\n" +
                     LocalizationService.Get(lang, "time_spent", botResult.TotalTime.TotalSeconds.ToString("F2"));

        if (!result.IsDraw)
        {
            var winReason = result.WinReason?.Contains("ответ") == true || result.WinReason?.Contains("answer") == true
                ? LocalizationService.Get(lang, "win_by_answers")
                : LocalizationService.Get(lang, "win_by_time");
            message += $"\n\n{winReason}";
        }

        try
        {
            await _bot.SendMessage(humanResult.TelegramId,
                message,
                parseMode: ParseMode.Html,
                replyMarkup: _keyboard.GetMainMenuReplyKeyboard(lang));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send game results to {TelegramId}", humanResult.TelegramId);
            // Try without HTML formatting
            await _bot.SendMessage(humanResult.TelegramId,
                message,
                replyMarkup: _keyboard.GetMainMenuReplyKeyboard(lang));
        }
    }
}
