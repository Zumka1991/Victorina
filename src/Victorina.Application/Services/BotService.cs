using Victorina.Application.Interfaces;
using Victorina.Domain.Entities;

namespace Victorina.Application.Services;

public interface IBotService
{
    Task<User> GetOrCreateBotOpponentAsync(string languageCode);
    Task<(bool isCorrect, int answerIndex, int timeMs)> GetBotAnswerAsync(BotDifficulty difficulty, int correctAnswerIndex);
}

public class BotService : IBotService
{
    private static readonly Random _random = new Random();
    private readonly IUserService _userService;

    // Fixed pool of bot IDs to avoid creating infinite bots
    private static readonly long[] BotTelegramIds =
    {
        -1000001, -1000002, -1000003, -1000004, -1000005,
        -1000006, -1000007, -1000008, -1000009, -1000010,
        -1000011, -1000012, -1000013, -1000014, -1000015,
        -1000016, -1000017, -1000018, -1000019, -1000020,
        -1000021, -1000022, -1000023, -1000024, -1000025,
        -1000026, -1000027, -1000028, -1000029, -1000030
    };

    public BotService(IUserService userService)
    {
        _userService = userService;
    }

    // Список случайных имен для ботов
    private static readonly Dictionary<string, string[]> BotNames = new()
    {
        ["ru"] = new[]
        {
            "Александр", "Дмитрий", "Михаил", "Андрей", "Сергей",
            "Екатерина", "Анна", "Мария", "Ольга", "Елена",
            "Иван", "Петр", "Владимир", "Николай", "Артем",
            "Наталья", "Татьяна", "Виктория", "Ирина", "Юлия"
        },
        ["en"] = new[]
        {
            "Alex", "Mike", "John", "David", "Chris",
            "Emma", "Sarah", "Lisa", "Kate", "Anna",
            "Robert", "James", "William", "Daniel", "Matthew",
            "Emily", "Jessica", "Jennifer", "Amanda", "Sophia"
        },
        ["hi"] = new[]
        {
            "Raj", "Amit", "Rahul", "Arjun", "Vikram",
            "Priya", "Anjali", "Neha", "Pooja", "Simran",
            "Rohan", "Karan", "Aditya", "Aman", "Varun",
            "Kavya", "Ishita", "Riya", "Shreya", "Diya"
        },
        ["pt"] = new[]
        {
            "João", "Pedro", "Lucas", "Gabriel", "Miguel",
            "Maria", "Ana", "Beatriz", "Julia", "Sofia",
            "Rafael", "Felipe", "Matheus", "Bruno", "Diego",
            "Laura", "Camila", "Isabela", "Leticia", "Fernanda"
        },
        ["fa"] = new[]
        {
            "علی", "محمد", "رضا", "حسین", "امیر",
            "فاطمه", "زهرا", "مریم", "سارا", "نازنین",
            "مهدی", "احمد", "سعید", "کاوه", "بابک",
            "لیلا", "نرگس", "پریسا", "شیدا", "نیلوفر"
        },
        ["de"] = new[]
        {
            "Max", "Paul", "Leon", "Lukas", "Felix",
            "Emma", "Mia", "Hannah", "Sophia", "Anna",
            "Tim", "Jonas", "Jan", "Nico", "Erik",
            "Lena", "Laura", "Sarah", "Marie", "Julia"
        },
        ["uz"] = new[]
        {
            "Aziz", "Bobur", "Jamshid", "Sardor", "Otabek",
            "Madina", "Nigora", "Zarina", "Dilnoza", "Feruza",
            "Jasur", "Rustam", "Shohruh", "Farruh", "Davron",
            "Kamola", "Gulnora", "Laylo", "Sevara", "Malika"
        }
    };

    public async Task<User> GetOrCreateBotOpponentAsync(string languageCode)
    {
        // Pick a random bot from the pool
        var botTelegramId = BotTelegramIds[_random.Next(BotTelegramIds.Length)];

        // Check if bot already exists
        var existingBot = await _userService.GetByTelegramIdAsync(botTelegramId);
        if (existingBot != null)
        {
            return existingBot;
        }

        // Create new bot if doesn't exist
        var difficulties = new[] { BotDifficulty.Easy, BotDifficulty.Medium, BotDifficulty.Hard };
        var difficulty = difficulties[_random.Next(difficulties.Length)];

        var names = BotNames.ContainsKey(languageCode) ? BotNames[languageCode] : BotNames["en"];
        var firstName = names[_random.Next(names.Length)];

        var bot = new User
        {
            TelegramId = botTelegramId,
            Username = $"bot_{Math.Abs(botTelegramId)}",
            FirstName = firstName,
            LanguageCode = languageCode,
            IsBot = true,
            BotDifficulty = difficulty,
            CreatedAt = DateTime.UtcNow,
            LastActiveAt = DateTime.UtcNow
        };

        return bot;
    }

    public Task<(bool isCorrect, int answerIndex, int timeMs)> GetBotAnswerAsync(
        BotDifficulty difficulty,
        int correctAnswerIndex)
    {
        bool isCorrect;
        int answerIndex;
        int timeMs;

        switch (difficulty)
        {
            case BotDifficulty.Easy:
                // 30% шанс правильного ответа
                isCorrect = _random.Next(100) < 30;
                // Медленный ответ: 8-15 секунд
                timeMs = _random.Next(8000, 15000);
                break;

            case BotDifficulty.Medium:
                // 60% шанс правильного ответа
                isCorrect = _random.Next(100) < 60;
                // Средний ответ: 4-8 секунд
                timeMs = _random.Next(4000, 8000);
                break;

            case BotDifficulty.Hard:
                // 90% шанс правильного ответа
                isCorrect = _random.Next(100) < 90;
                // Быстрый ответ: 2-5 секунд
                timeMs = _random.Next(2000, 5000);
                break;

            default:
                isCorrect = _random.Next(100) < 50;
                timeMs = _random.Next(5000, 10000);
                break;
        }

        if (isCorrect)
        {
            answerIndex = correctAnswerIndex;
        }
        else
        {
            // Выбираем случайный неправильный ответ
            do
            {
                answerIndex = _random.Next(4);
            } while (answerIndex == correctAnswerIndex);
        }

        return Task.FromResult((isCorrect, answerIndex, timeMs));
    }
}
