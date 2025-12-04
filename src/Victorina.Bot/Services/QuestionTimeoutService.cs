using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Victorina.Application.Interfaces;
using Victorina.Application.Models;

namespace Victorina.Bot.Services;

public class QuestionTimeoutService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ITelegramBotClient _bot;
    private readonly KeyboardService _keyboard;
    private readonly ILogger<QuestionTimeoutService> _logger;

    public QuestionTimeoutService(
        IServiceProvider serviceProvider,
        ITelegramBotClient bot,
        KeyboardService keyboard,
        ILogger<QuestionTimeoutService> logger)
    {
        _serviceProvider = serviceProvider;
        _bot = bot;
        _keyboard = keyboard;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Question timeout service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckTimeoutsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking timeouts");
            }

            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task<string> GetUserLanguageAsync(long telegramId, IUserService userService)
    {
        var user = await userService.GetByTelegramIdAsync(telegramId);
        return user?.LanguageCode ?? "ru";
    }

    private async Task CheckTimeoutsAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var gameService = scope.ServiceProvider.GetRequiredService<IGameService>();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var timeouts = await gameService.CheckAndHandleTimeoutsAsync();
        var processedGames = new HashSet<int>();

        foreach (var timeout in timeouts)
        {
            try
            {
                var lang = await GetUserLanguageAsync(timeout.TelegramId, userService);

                // Отправляем сообщение "Время вышло" только тому, кто не ответил
                await _bot.SendMessage(
                    timeout.TelegramId,
                    LocalizationService.Get(lang, "time_up", timeout.CorrectAnswer),
                    parseMode: ParseMode.Markdown,
                    cancellationToken: ct);

                if (timeout.GameResult != null)
                {
                    // Отправляем результаты игры
                    await SendGameResultAsync(timeout.TelegramId, timeout.GameResult, userService, ct);

                    // Отправляем результаты игрокам, которые ответили вовремя (только один раз на игру)
                    if (!processedGames.Contains(timeout.GameId))
                    {
                        foreach (var waitingPlayer in timeout.PlayersWaitingForNextQuestion)
                        {
                            await SendGameResultAsync(waitingPlayer, timeout.GameResult, userService, ct);
                        }
                        processedGames.Add(timeout.GameId);
                    }
                }
                else if (!timeout.IsLastQuestion)
                {
                    // Получаем сессию для отправки следующего вопроса
                    var session = await gameService.GetGameByIdAsync(timeout.GameId);
                    if (session != null && session.Status == Domain.Enums.GameStatus.InProgress)
                    {
                        await Task.Delay(1500, ct);

                        // Отправляем следующий вопрос тому, у кого вышло время (в его языке)
                        if (session.Players.TryGetValue(timeout.TelegramId, out var timeoutPlayer))
                        {
                            var playerQuestion = timeoutPlayer.Questions.Count > session.CurrentQuestionIndex
                                ? timeoutPlayer.Questions[session.CurrentQuestionIndex]
                                : session.Questions[session.CurrentQuestionIndex];
                            await SendQuestionAsync(timeout.TelegramId, lang, playerQuestion,
                                session.CurrentQuestionIndex + 1, session.Questions.Count, ct);
                        }

                        // Отправляем следующий вопрос игрокам, которые ответили вовремя (только один раз на игру)
                        if (!processedGames.Contains(timeout.GameId))
                        {
                            foreach (var waitingPlayerId in timeout.PlayersWaitingForNextQuestion)
                            {
                                if (session.Players.TryGetValue(waitingPlayerId, out var waitingPlayer))
                                {
                                    var playerQuestion = waitingPlayer.Questions.Count > session.CurrentQuestionIndex
                                        ? waitingPlayer.Questions[session.CurrentQuestionIndex]
                                        : session.Questions[session.CurrentQuestionIndex];
                                    var playerLang = await GetUserLanguageAsync(waitingPlayerId, userService);
                                    await SendQuestionAsync(waitingPlayerId, playerLang, playerQuestion,
                                        session.CurrentQuestionIndex + 1, session.Questions.Count, ct);
                                }
                            }
                            processedGames.Add(timeout.GameId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending timeout notification to {TelegramId}", timeout.TelegramId);
            }
        }
    }

    private async Task SendQuestionAsync(long chatId, string lang, Application.Models.GameSessionQuestion question,
        int questionNumber, int totalQuestions, CancellationToken ct)
    {
        var questionText = LocalizationService.Get(lang, "question", questionNumber, totalQuestions) + $"\n\n{question.Text}";

        if (!string.IsNullOrEmpty(question.ImageUrl))
        {
            // For simplicity, just send text if there's an image URL issue
            // The main handler will handle image sending properly
        }

        await _bot.SendMessage(chatId,
            questionText,
            parseMode: ParseMode.Markdown,
            replyMarkup: _keyboard.GetQuestionKeyboard(question.Answers),
            cancellationToken: ct);
    }

    private async Task SendGameResultAsync(long telegramId, GameResult result, IUserService userService, CancellationToken ct)
    {
        var lang = await GetUserLanguageAsync(telegramId, userService);
        var playerResult = result.Player1.TelegramId == telegramId ? result.Player1 : result.Player2;
        var opponent = result.Player1.TelegramId == telegramId ? result.Player2 : result.Player1;

        var isWinner = result.WinnerTelegramId == telegramId;

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

        var opponentFlag = CountryService.GetFlag(opponent.CountryCode);
        var opponentName = opponent.GetDisplayName();

        var message = $"{title}\n\n" +
                     LocalizationService.Get(lang, "your_result") + $"\n" +
                     LocalizationService.Get(lang, "correct_answers", playerResult.CorrectAnswers) + $"\n" +
                     LocalizationService.Get(lang, "time_spent", playerResult.TotalTime.TotalSeconds.ToString("F2")) + $"\n\n" +
                     LocalizationService.Get(lang, "opponent_result", opponentFlag, opponentName) + $"\n" +
                     LocalizationService.Get(lang, "correct_answers", opponent.CorrectAnswers) + $"\n" +
                     LocalizationService.Get(lang, "time_spent", opponent.TotalTime.TotalSeconds.ToString("F2"));

        if (!result.IsDraw)
        {
            var winReason = result.WinReason?.Contains("ответ") == true || result.WinReason?.Contains("answer") == true
                ? LocalizationService.Get(lang, "win_by_answers")
                : LocalizationService.Get(lang, "win_by_time");
            message += $"\n\n_{winReason}_";
        }

        await _bot.SendMessage(telegramId,
            message,
            parseMode: ParseMode.Markdown,
            replyMarkup: _keyboard.GetMainMenuReplyKeyboard(lang),
            cancellationToken: ct);
    }
}
