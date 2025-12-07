namespace Victorina.Domain.Entities;

/// <summary>
/// Tracks which questions have been shown to each user to avoid repetition
/// </summary>
public class UserQuestionHistory
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid QuestionTranslationGroupId { get; set; }
    public DateTime ShownAt { get; set; }
}
