namespace Victorina.Domain.Entities;

public class GameQuestion
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public int QuestionId { get; set; }
    public int OrderIndex { get; set; }
    public string ShuffledAnswersJson { get; set; } = "[]";

    public Game Game { get; set; } = null!;
    public Question Question { get; set; } = null!;
}
