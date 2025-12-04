namespace Victorina.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public long TelegramId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? CountryCode { get; set; } // ISO 3166-1 alpha-2 (RU, UA, BY, KZ, etc.)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActiveAt { get; set; } = DateTime.UtcNow;

    // Статистика
    public int GamesPlayed { get; set; }
    public int GamesWon { get; set; }
    public int TotalCorrectAnswers { get; set; }

    // Навигация
    public ICollection<Friendship> FriendshipsInitiated { get; set; } = new List<Friendship>();
    public ICollection<Friendship> FriendshipsReceived { get; set; } = new List<Friendship>();
    public ICollection<GamePlayer> GamePlayers { get; set; } = new List<GamePlayer>();
}
