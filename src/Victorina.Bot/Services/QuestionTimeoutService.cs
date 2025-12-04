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

    private async Task CheckTimeoutsAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var gameService = scope.ServiceProvider.GetRequiredService<IGameService>();

        var timeouts = await gameService.CheckAndHandleTimeoutsAsync();
        var processedGames = new HashSet<int>();

        foreach (var timeout in timeouts)
        {
            try
            {
                // Отправляем сообщение "Время вышло" только тому, кто не ответил
                await _bot.SendMessage(
                    timeout.TelegramId,
                    $"⏱ *Время вышло!*\n\nПравильный ответ: *{timeout.CorrectAnswer}*",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: ct);

                if (timeout.GameResult != null)
                {
                    // Отправляем результаты игры
                    await SendGameResultAsync(timeout.TelegramId, timeout.GameResult, ct);

                    // Отправляем результаты игрокам, которые ответили вовремя (только один раз на игру)
                    if (!processedGames.Contains(timeout.GameId))
                    {
                        foreach (var waitingPlayer in timeout.PlayersWaitingForNextQuestion)
                        {
                            await SendGameResultAsync(waitingPlayer, timeout.GameResult, ct);
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
                        var nextQuestion = session.Questions[session.CurrentQuestionIndex];

                        await Task.Delay(1500, ct);

                        // Отправляем следующий вопрос тому, у кого вышло время
                        await SendQuestionAsync(timeout.TelegramId, nextQuestion,
                            session.CurrentQuestionIndex + 1, session.Questions.Count, ct);

                        // Отправляем следующий вопрос игрокам, которые ответили вовремя (только один раз на игру)
                        if (!processedGames.Contains(timeout.GameId))
                        {
                            foreach (var waitingPlayer in timeout.PlayersWaitingForNextQuestion)
                            {
                                await SendQuestionAsync(waitingPlayer, nextQuestion,
                                    session.CurrentQuestionIndex + 1, session.Questions.Count, ct);
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

    private async Task SendQuestionAsync(long chatId, Application.Models.GameSessionQuestion question,
        int questionNumber, int totalQuestions, CancellationToken ct)
    {
        var questionText = $"❓ *Вопрос {questionNumber}/{totalQuestions}*\n\n{question.Text}";

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

    private async Task SendGameResultAsync(long telegramId, GameResult result, CancellationToken ct)
    {
        var playerResult = result.Player1.TelegramId == telegramId ? result.Player1 : result.Player2;
        var opponent = result.Player1.TelegramId == telegramId ? result.Player2 : result.Player1;

        var isWinner = result.WinnerTelegramId == telegramId;

        string emoji, title;
        if (result.IsDraw)
        {
            emoji = "🤝";
            title = "Ничья!";
        }
        else if (isWinner)
        {
            emoji = "🏆";
            title = "Вы победили!";
        }
        else
        {
            emoji = "😔";
            title = "Вы проиграли";
        }

        var opponentFlag = CountryService.GetFlag(opponent.CountryCode);
        var opponentName = opponent.GetDisplayName();

        var message = $"{emoji} *{title}*\n\n" +
                     $"📊 *Ваш результат:*\n" +
                     $"✅ Правильных: {playerResult.CorrectAnswers}\n" +
                     $"⏱ Время: {playerResult.TotalTime.TotalSeconds:F2} сек\n\n" +
                     $"📊 *Соперник:* {opponentFlag} {opponentName}\n" +
                     $"✅ Правильных: {opponent.CorrectAnswers}\n" +
                     $"⏱ Время: {opponent.TotalTime.TotalSeconds:F2} сек";

        if (!result.IsDraw)
        {
            message += $"\n\n_{result.WinReason}_";
        }

        await _bot.SendMessage(telegramId,
            message,
            parseMode: ParseMode.Markdown,
            replyMarkup: _keyboard.GetMainMenuReplyKeyboard(),
            cancellationToken: ct);
    }
}
