using Victorina.Application.Models;
using Victorina.Domain.Entities;

namespace Victorina.Application.Interfaces;

public interface IGameService
{
    Task<GameSession?> FindQuickGameAsync(long telegramId, int? categoryId = null);
    Task<GameSession> CreateQuickGameAsync(long telegramId, int? categoryId = null);
    Task<GameSession> CreateFriendGameAsync(long creatorTelegramId, long friendTelegramId, int? categoryId = null);
    Task<GameSession?> JoinGameAsync(int gameId, long telegramId);
    Task<bool> SetPlayerReadyAsync(int gameId, long telegramId);
    Task<GameSession?> GetActiveGameAsync(long telegramId);
    Task<GameSession?> GetGameByIdAsync(int gameId);
    Task<GameSessionQuestion?> GetCurrentQuestionAsync(int gameId);
    Task<AnswerResult> SubmitAnswerAsync(int gameId, long telegramId, int answerIndex);
    Task<bool> MoveToNextQuestionAsync(int gameId);
    Task<GameResult?> FinishGameAsync(int gameId);
    Task CancelGameAsync(int gameId);
    Task<List<TimeoutInfo>> CheckAndHandleTimeoutsAsync();
    Task<UserStats> GetUserStatsAsync(long telegramId);
    Task<List<LeaderboardEntry>> GetLeaderboardAsync(int count = 10);
    Task<(int Rank, UserStats Stats)?> GetUserRankAsync(long telegramId);
}

public class UserStats
{
    public int GamesPlayed { get; set; }
    public int GamesWon { get; set; }
    public int TotalCorrectAnswers { get; set; }
    public double WinRate => GamesPlayed > 0 ? (double)GamesWon / GamesPlayed * 100 : 0;
}

public class LeaderboardEntry
{
    public int Rank { get; set; }
    public long TelegramId { get; set; }
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public int GamesPlayed { get; set; }
    public int GamesWon { get; set; }
    public int TotalCorrectAnswers { get; set; }
    public double WinRate => GamesPlayed > 0 ? (double)GamesWon / GamesPlayed * 100 : 0;
}

public class TimeoutInfo
{
    public int GameId { get; set; }
    public long TelegramId { get; set; }
    public string CorrectAnswer { get; set; } = string.Empty;
    public bool BothTimedOut { get; set; }
    public bool IsLastQuestion { get; set; }
    public GameResult? GameResult { get; set; }
    public List<long> PlayersWaitingForNextQuestion { get; set; } = new();
}
