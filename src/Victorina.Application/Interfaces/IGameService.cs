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
}

public class UserStats
{
    public int GamesPlayed { get; set; }
    public int GamesWon { get; set; }
    public int TotalCorrectAnswers { get; set; }
    public double WinRate => GamesPlayed > 0 ? (double)GamesWon / GamesPlayed * 100 : 0;
}
