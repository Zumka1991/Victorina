using Victorina.Domain.Enums;

namespace Victorina.Application.Models;

public class GameSession
{
    public int GameId { get; set; }
    public Guid GameGuid { get; set; }
    public GameStatus Status { get; set; }
    public GameType Type { get; set; }
    public int QuestionTimeSeconds { get; set; }
    public List<GameSessionQuestion> Questions { get; set; } = new();
    public Dictionary<long, PlayerSession> Players { get; set; } = new();
    public int CurrentQuestionIndex { get; set; }
    public DateTime? QuestionStartedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PlayerSession
{
    public int UserId { get; set; }
    public long TelegramId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int GamePlayerId { get; set; }
    public bool IsReady { get; set; }
    public int CorrectAnswers { get; set; }
    public long TotalTimeMs { get; set; }
    public int CurrentQuestionIndex { get; set; }
    public int? LastMessageId { get; set; }
    public DateTime? LastAnswerTime { get; set; }
}

public class GameSessionQuestion
{
    public int QuestionId { get; set; }
    public int GameQuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string[] Answers { get; set; } = Array.Empty<string>();
    public string CorrectAnswer { get; set; } = string.Empty;
    public int CorrectAnswerIndex { get; set; }
    public string? Explanation { get; set; }
}
