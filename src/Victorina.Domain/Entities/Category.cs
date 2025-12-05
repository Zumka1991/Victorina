namespace Victorina.Domain.Entities;

public class Category
{
    public int Id { get; set; }
    public string LanguageCode { get; set; } = "ru"; // ISO 639-1 (ru, hi, pt, fa, de, uz)
    public Guid? TranslationGroupId { get; set; } // Groups translations of the same category
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Emoji { get; set; }
    public string CategoryGroup { get; set; } = "general"; // general, special, popular
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Question> Questions { get; set; } = new List<Question>();
}
