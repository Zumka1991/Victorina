using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Victorina.Application.Interfaces;
using Victorina.Domain.Enums;

namespace Victorina.Application.Services;

public class BotAnswerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BotAnswerService> _logger;
    private const int CheckIntervalMs = 500; // Check every 500ms for responsiveness

    public BotAnswerService(
        IServiceProvider serviceProvider,
        ILogger<BotAnswerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BotAnswerService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBotAnswersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing bot answers");
            }

            await Task.Delay(CheckIntervalMs, stoppingToken);
        }

        _logger.LogInformation("BotAnswerService stopped");
    }

    private async Task ProcessBotAnswersAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var sessionStore = scope.ServiceProvider.GetRequiredService<GameSessionStore>();
        var gameService = scope.ServiceProvider.GetRequiredService<IGameService>();
        var botService = scope.ServiceProvider.GetRequiredService<IBotService>();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var activeSessions = sessionStore.GetActiveGameSessions().ToList();

        foreach (var session in activeSessions)
        {
            // Only process games that have started and have a current question
            if (session.Status != GameStatus.InProgress ||
                session.QuestionStartedAt == null ||
                session.CurrentQuestionIndex >= session.Questions.Count)
                continue;

            // Find players who haven't answered yet
            var playersWhoNeedToAnswer = session.Players.Values
                .Where(p => p.CurrentQuestionIndex == session.CurrentQuestionIndex)
                .ToList();

            if (playersWhoNeedToAnswer.Count == 0)
                continue;

            foreach (var player in playersWhoNeedToAnswer)
            {
                // Check if this player is a bot
                var user = await userService.GetByTelegramIdAsync(player.TelegramId);
                if (user == null || !user.IsBot || !user.BotDifficulty.HasValue)
                    continue;

                var elapsedMs = (DateTime.UtcNow - session.QuestionStartedAt.Value).TotalMilliseconds;

                // Get the current question
                var currentQuestion = session.Questions[session.CurrentQuestionIndex];

                // Get bot's answer based on difficulty
                var botAnswer = await botService.GetBotAnswerAsync(
                    user.BotDifficulty.Value,
                    currentQuestion.CorrectAnswerIndex);

                // Check if it's time for the bot to answer
                if (elapsedMs >= botAnswer.timeMs)
                {
                    try
                    {
                        _logger.LogInformation(
                            "Bot {BotName} answering question {QuestionIndex} in game {GameId} after {TimeMs}ms",
                            player.FirstName, session.CurrentQuestionIndex, session.GameId, elapsedMs);

                        var result = await gameService.SubmitAnswerAsync(
                            session.GameId,
                            player.TelegramId,
                            botAnswer.answerIndex);

                        _logger.LogInformation(
                            "Bot {BotName} submitted answer for question {QuestionIndex} in game {GameId}, correct: {IsCorrect}",
                            player.FirstName, session.CurrentQuestionIndex, session.GameId, result.IsCorrect);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Error submitting bot answer for {BotName} in game {GameId}",
                            player.FirstName, session.GameId);
                    }
                }
            }
        }
    }
}
