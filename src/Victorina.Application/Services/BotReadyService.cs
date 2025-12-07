using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Victorina.Application.Interfaces;
using Victorina.Domain.Enums;

namespace Victorina.Application.Services;

/// <summary>
/// Service that automatically marks bot players as ready when they join a game
/// </summary>
public class BotReadyService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BotReadyService> _logger;
    private readonly HashSet<int> _readiedGames = new();
    private const int CheckIntervalMs = 500; // Check every 500ms

    public BotReadyService(
        IServiceProvider serviceProvider,
        ILogger<BotReadyService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BotReadyService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await MakeBotsReadyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BotReadyService");
            }

            await Task.Delay(CheckIntervalMs, stoppingToken);
        }

        _logger.LogInformation("BotReadyService stopped");
    }

    private async Task MakeBotsReadyAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var sessionStore = scope.ServiceProvider.GetRequiredService<GameSessionStore>();
        var gameService = scope.ServiceProvider.GetRequiredService<IGameService>();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        // Get all sessions waiting for ready
        var waitingForReadySessions = sessionStore.GetAllSessions()
            .Where(s => s.Status == GameStatus.WaitingForReady && s.Players.Count == 2)
            .ToList();

        foreach (var session in waitingForReadySessions)
        {
            // Skip if already processed
            if (_readiedGames.Contains(session.GameId))
                continue;

            var playersList = session.Players.Values.ToList();

            // Check each player to see if they are a bot
            foreach (var player in playersList)
            {
                // Skip if player is already ready
                if (player.IsReady)
                    continue;

                var user = await userService.GetByTelegramIdAsync(player.TelegramId);
                if (user != null && user.IsBot)
                {
                    try
                    {
                        // Make bot ready
                        await gameService.SetPlayerReadyAsync(session.GameId, player.TelegramId);

                        _logger.LogInformation(
                            "Bot {BotName} marked as ready in game {GameId}",
                            player.FirstName, session.GameId);

                        // Mark this game as processed
                        _readiedGames.Add(session.GameId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Failed to mark bot {BotName} as ready in game {GameId}",
                            player.FirstName, session.GameId);
                    }
                }
            }
        }

        // Clean up old processed games
        var activeGameIds = waitingForReadySessions.Select(s => s.GameId).ToHashSet();
        _readiedGames.RemoveWhere(id => !activeGameIds.Contains(id));
    }
}
