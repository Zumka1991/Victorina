namespace Victorina.Application.Models;

public class GameResult
{
    public int GameId { get; set; }
    public PlayerResult Player1 { get; set; } = null!;
    public PlayerResult Player2 { get; set; } = null!;
    public long? WinnerTelegramId { get; set; }
    public bool IsDraw { get; set; }
    public string WinReason { get; set; } = string.Empty;
}

public class PlayerResult
{
    public long TelegramId { get; set; }
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? CountryCode { get; set; }
    public int CorrectAnswers { get; set; }
    public TimeSpan TotalTime { get; set; }

    public string GetDisplayName()
    {
        var nameParts = new List<string>();
        if (!string.IsNullOrEmpty(FirstName))
            nameParts.Add(FirstName);
        if (!string.IsNullOrEmpty(LastName))
            nameParts.Add(LastName);

        var fullName = nameParts.Count > 0 ? string.Join(" ", nameParts) : "Игрок";

        // Don't show username for bots
        if (!string.IsNullOrEmpty(Username) && !Username.StartsWith("bot_"))
            return $"{fullName} (@{Username})";

        return fullName;
    }
}
