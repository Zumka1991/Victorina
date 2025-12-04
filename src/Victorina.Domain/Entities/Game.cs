using Victorina.Domain.Enums;

namespace Victorina.Domain.Entities;

public class Game
{
    public int Id { get; set; }
    public Guid GameGuid { get; set; } = Guid.NewGuid();
    public GameStatus Status { get; set; } = GameStatus.WaitingForPlayers;
    public GameType Type { get; set; }
    public int? CategoryId { get; set; }
    public int QuestionTimeSeconds { get; set; } = 15;
    public int TotalQuestions { get; set; } = 10;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public int? WinnerId { get; set; }

    public Category? Category { get; set; }
    public User? Winner { get; set; }
    public ICollection<GamePlayer> Players { get; set; } = new List<GamePlayer>();
    public ICollection<GameQuestion> Questions { get; set; } = new List<GameQuestion>();
}
