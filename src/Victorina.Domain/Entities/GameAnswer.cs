namespace Victorina.Domain.Entities;

public class GameAnswer
{
    public int Id { get; set; }
    public int GamePlayerId { get; set; }
    public int GameQuestionId { get; set; }
    public string SelectedAnswer { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public long TimeMs { get; set; }
    public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;

    public GamePlayer GamePlayer { get; set; } = null!;
    public GameQuestion GameQuestion { get; set; } = null!;
}
