using Victorina.Application.Models;
using Victorina.Domain.Entities;

namespace Victorina.Application.Interfaces;

public interface IGameService
{
    Task<GameSession?> FindQuickGameAsync(long telegramId);
    Task<GameSession> CreateQuickGameAsync(long telegramId);
    Task<GameSession> CreateFriendGameAsync(long creatorTelegramId, long friendTelegramId);
    Task<GameSession?> JoinGameAsync(int gameId, long telegramId);
    Task<bool> SetPlayerReadyAsync(int gameId, long telegramId);
    Task<GameSession?> GetActiveGameAsync(long telegramId);
    Task<GameSession?> GetGameByIdAsync(int gameId);
    Task<GameSessionQuestion?> GetCurrentQuestionAsync(int gameId);
    Task<AnswerResult> SubmitAnswerAsync(int gameId, long telegramId, int answerIndex);
    Task<bool> MoveToNextQuestionAsync(int gameId);
    Task<GameResult?> FinishGameAsync(int gameId);
    Task CancelGameAsync(int gameId);
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
