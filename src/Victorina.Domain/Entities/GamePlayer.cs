namespace Victorina.Domain.Entities;

public class GamePlayer
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public int UserId { get; set; }
    public int CorrectAnswers { get; set; }
    public long TotalTimeMs { get; set; }
    public bool IsReady { get; set; }
    public int CurrentQuestionIndex { get; set; }

    public Game Game { get; set; } = null!;
    public User User { get; set; } = null!;
    public ICollection<GameAnswer> Answers { get; set; } = new List<GameAnswer>();
}
