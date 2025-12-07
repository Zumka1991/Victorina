using Victorina.Domain.Entities;

namespace Victorina.Application.Services;

public interface IBotService
{
    Task<User> CreateBotOpponentAsync(string languageCode);
    Task<(bool isCorrect, int answerIndex, int timeMs)> GetBotAnswerAsync(BotDifficulty difficulty, int correctAnswerIndex);
}

public class BotService : IBotService
{
    private static readonly Random _random = new Random();

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

    public Task<User> CreateBotOpponentAsync(string languageCode)
    {
        // Выбираем случайную сложность
        var difficulties = new[] { BotDifficulty.Easy, BotDifficulty.Medium, BotDifficulty.Hard };
        var difficulty = difficulties[_random.Next(difficulties.Length)];

        // Выбираем случайное имя
        var names = BotNames.ContainsKey(languageCode) ? BotNames[languageCode] : BotNames["en"];
        var firstName = names[_random.Next(names.Length)];

        // Создаем бота с уникальным TelegramId (отрицательный для ботов)
        var bot = new User
        {
            TelegramId = -_random.Next(1000000, 9999999), // Negative IDs for bots
            Username = $"bot_{Guid.NewGuid().ToString()[..8]}",
            FirstName = firstName,
            LanguageCode = languageCode,
            IsBot = true,
            BotDifficulty = difficulty,
            CreatedAt = DateTime.UtcNow,
            LastActiveAt = DateTime.UtcNow
        };

        return Task.FromResult(bot);
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
