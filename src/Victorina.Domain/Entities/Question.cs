namespace Victorina.Domain.Entities;

public class Question
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public string WrongAnswer1 { get; set; } = string.Empty;
    public string WrongAnswer2 { get; set; } = string.Empty;
    public string WrongAnswer3 { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Category Category { get; set; } = null!;

    public string[] GetShuffledAnswers()
    {
        var answers = new[] { CorrectAnswer, WrongAnswer1, WrongAnswer2, WrongAnswer3 };
        return answers.OrderBy(_ => Random.Shared.Next()).ToArray();
    }
}
