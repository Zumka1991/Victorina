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
    public string Username { get; set; } = string.Empty;
    public int CorrectAnswers { get; set; }
    public TimeSpan TotalTime { get; set; }
}
