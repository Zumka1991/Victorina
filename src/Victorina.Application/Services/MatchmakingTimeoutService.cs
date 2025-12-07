using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Victorina.Application.Interfaces;
using Victorina.Domain.Enums;

namespace Victorina.Application.Services;

public class MatchmakingTimeoutService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MatchmakingTimeoutService> _logger;
    private const int CheckIntervalSeconds = 1; // Check every 1 second (for testing)
    private const int MatchmakingTimeoutSeconds = 3; // 3 seconds timeout (for testing)

    public MatchmakingTimeoutService(
        IServiceProvider serviceProvider,
        ILogger<MatchmakingTimeoutService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MatchmakingTimeoutService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckWaitingGamesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking waiting games for timeout");
            }

            await Task.Delay(TimeSpan.FromSeconds(CheckIntervalSeconds), stoppingToken);
        }

        _logger.LogInformation("MatchmakingTimeoutService stopped");
    }

    private async Task CheckWaitingGamesAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var sessionStore = scope.ServiceProvider.GetRequiredService<GameSessionStore>();
        var gameService = scope.ServiceProvider.GetRequiredService<IGameService>();
        var botService = scope.ServiceProvider.GetRequiredService<IBotService>();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<Infrastructure.Data.VictorinaDbContext>();

        var waitingSessions = sessionStore.GetWaitingSessions().ToList();

        foreach (var session in waitingSessions)
        {
            // Only check quick games with 1 player
            if (session.Type != GameType.QuickGame || session.Players.Count != 1)
                continue;

            var waitingTime = DateTime.UtcNow - session.CreatedAt;
            if (waitingTime.TotalSeconds < MatchmakingTimeoutSeconds)
                continue;

            _logger.LogInformation(
                "Game {GameId} has been waiting for {Seconds} seconds. Creating bot opponent.",
                session.GameId, waitingTime.TotalSeconds);

            try
            {
                // Get the first (and only) player
                var humanPlayer = session.Players.Values.First();

                // Get or create a bot opponent from the pool
                var botUser = await botService.GetOrCreateBotOpponentAsync(humanPlayer.LanguageCode);

                // Save bot to database using GetOrCreateUserAsync
                var savedBot = await userService.GetOrCreateUserAsync(
                    botUser.TelegramId,
                    botUser.Username,
                    botUser.FirstName,
                    botUser.LastName);

                // Update bot-specific fields
                savedBot.IsBot = true;
                savedBot.BotDifficulty = botUser.BotDifficulty;
                savedBot.LanguageCode = botUser.LanguageCode;
                await dbContext.SaveChangesAsync();

                // Join bot to the game
                await gameService.JoinGameAsync(session.GameId, botUser.TelegramId);

                _logger.LogInformation(
                    "Bot {BotName} (difficulty: {Difficulty}) joined game {GameId}",
                    savedBot.FirstName, savedBot.BotDifficulty, session.GameId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bot opponent for game {GameId}", session.GameId);
            }
        }
    }
}
