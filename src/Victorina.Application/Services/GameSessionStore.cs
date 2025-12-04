using System.Collections.Concurrent;
using Victorina.Application.Models;

namespace Victorina.Application.Services;

public class GameSessionStore
{
    private readonly ConcurrentDictionary<int, GameSession> _sessions = new();
    private readonly ConcurrentDictionary<long, int> _playerToGame = new();

    public void AddSession(GameSession session)
    {
        _sessions[session.GameId] = session;
        foreach (var player in session.Players.Values)
        {
            _playerToGame[player.TelegramId] = session.GameId;
        }
    }

    public GameSession? GetSession(int gameId)
    {
        _sessions.TryGetValue(gameId, out var session);
        return session;
    }

    public GameSession? GetSessionByPlayer(long telegramId)
    {
        if (_playerToGame.TryGetValue(telegramId, out var gameId))
        {
            return GetSession(gameId);
        }
        return null;
    }

    public void AddPlayerToSession(int gameId, PlayerSession player)
    {
        if (_sessions.TryGetValue(gameId, out var session))
        {
            session.Players[player.TelegramId] = player;
            _playerToGame[player.TelegramId] = gameId;
        }
    }

    public void RemoveSession(int gameId)
    {
        if (_sessions.TryRemove(gameId, out var session))
        {
            foreach (var player in session.Players.Values)
            {
                _playerToGame.TryRemove(player.TelegramId, out _);
            }
        }
    }

    public void RemovePlayer(long telegramId)
    {
        _playerToGame.TryRemove(telegramId, out _);
    }

    public IEnumerable<GameSession> GetWaitingSessions()
    {
        return _sessions.Values.Where(s =>
            s.Status == Domain.Enums.GameStatus.WaitingForPlayers &&
            s.Type == Domain.Enums.GameType.QuickGame);
    }
}
