namespace Victorina.Application.Models;

public class AnswerResult
{
    public bool IsCorrect { get; set; }
    public string CorrectAnswer { get; set; } = string.Empty;
    public long TimeMs { get; set; }
    public bool BothAnswered { get; set; }
    public OpponentAnswerInfo? OpponentAnswer { get; set; }
}

public class OpponentAnswerInfo
{
    public bool IsCorrect { get; set; }
    public long TimeMs { get; set; }
}
