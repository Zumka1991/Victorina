using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Victorina.Application.Interfaces;
using Victorina.Application.Models;
using Victorina.Domain.Entities;
using Victorina.Domain.Enums;
using Victorina.Infrastructure.Data;

namespace Victorina.Application.Services;

public class GameService : IGameService
{
    private readonly VictorinaDbContext _context;
    private readonly GameSessionStore _sessionStore;
    private readonly IUserService _userService;
    private readonly IQuestionService _questionService;

    public GameService(
        VictorinaDbContext context,
        GameSessionStore sessionStore,
        IUserService userService,
        IQuestionService questionService)
    {
        _context = context;
        _sessionStore = sessionStore;
        _userService = userService;
        _questionService = questionService;
    }

    public async Task<GameSession?> FindQuickGameAsync(long telegramId, int? categoryId = null)
    {
        var waitingSession = _sessionStore.GetWaitingSessions()
            .FirstOrDefault(s => !s.Players.ContainsKey(telegramId) &&
                                 s.CategoryId == categoryId);

        return waitingSession;
    }

    public async Task<GameSession> CreateQuickGameAsync(long telegramId, int? categoryId = null)
    {
        var user = await _userService.GetByTelegramIdAsync(telegramId);
        if (user == null) throw new InvalidOperationException("User not found");

        var settings = await GetGameSettingsAsync();
        var questions = await _questionService.GetRandomQuestionsAsync(settings.QuestionsCount, categoryId);

        string? categoryName = null;
        if (categoryId.HasValue)
        {
            var category = await _context.Categories.FindAsync(categoryId.Value);
            categoryName = category?.Name;
        }

        var game = new Game
        {
            Type = GameType.QuickGame,
            Status = GameStatus.WaitingForPlayers,
            CategoryId = categoryId,
            QuestionTimeSeconds = settings.QuestionTimeSeconds,
            TotalQuestions = settings.QuestionsCount,
            CreatedAt = DateTime.UtcNow
        };

        _context.Games.Add(game);
        await _context.SaveChangesAsync();

        var gamePlayer = new GamePlayer
        {
            GameId = game.Id,
            UserId = user.Id,
            IsReady = false
        };
        _context.GamePlayers.Add(gamePlayer);

        var gameQuestions = new List<GameQuestion>();
        for (int i = 0; i < questions.Count; i++)
        {
            var q = questions[i];
            var shuffled = q.GetShuffledAnswers();
            var gq = new GameQuestion
            {
                GameId = game.Id,
                QuestionId = q.Id,
                OrderIndex = i,
                ShuffledAnswersJson = JsonSerializer.Serialize(shuffled)
            };
            gameQuestions.Add(gq);
            _context.GameQuestions.Add(gq);
        }

        await _context.SaveChangesAsync();

        var session = new GameSession
        {
            GameId = game.Id,
            GameGuid = game.GameGuid,
            Status = GameStatus.WaitingForPlayers,
            Type = GameType.QuickGame,
            CategoryId = categoryId,
            CategoryName = categoryName,
            QuestionTimeSeconds = settings.QuestionTimeSeconds,
            CreatedAt = DateTime.UtcNow,
            Questions = gameQuestions.Select((gq, idx) =>
            {
                var q = questions[idx];
                var answers = JsonSerializer.Deserialize<string[]>(gq.ShuffledAnswersJson)!;
                return new GameSessionQuestion
                {
                    QuestionId = q.Id,
                    GameQuestionId = gq.Id,
                    Text = q.Text,
                    Answers = answers,
                    CorrectAnswer = q.CorrectAnswer,
                    CorrectAnswerIndex = Array.IndexOf(answers, q.CorrectAnswer),
                    Explanation = q.Explanation,
                    ImageUrl = q.ImageUrl
                };
            }).ToList()
        };

        session.Players[telegramId] = new PlayerSession
        {
            UserId = user.Id,
            TelegramId = telegramId,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            CountryCode = user.CountryCode,
            GamePlayerId = gamePlayer.Id,
            IsReady = false
        };

        _sessionStore.AddSession(session);

        return session;
    }

    public async Task<GameSession> CreateFriendGameAsync(long creatorTelegramId, long friendTelegramId, int? categoryId = null)
    {
        var creator = await _userService.GetByTelegramIdAsync(creatorTelegramId);
        var friend = await _userService.GetByTelegramIdAsync(friendTelegramId);

        if (creator == null || friend == null)
            throw new InvalidOperationException("User not found");

        var settings = await GetGameSettingsAsync();
        var questions = await _questionService.GetRandomQuestionsAsync(settings.QuestionsCount, categoryId);

        string? categoryName = null;
        if (categoryId.HasValue)
        {
            var category = await _context.Categories.FindAsync(categoryId.Value);
            categoryName = category?.Name;
        }

        var game = new Game
        {
            Type = GameType.FriendGame,
            Status = GameStatus.WaitingForPlayers,
            CategoryId = categoryId,
            QuestionTimeSeconds = settings.QuestionTimeSeconds,
            TotalQuestions = settings.QuestionsCount,
            CreatedAt = DateTime.UtcNow
        };

        _context.Games.Add(game);
        await _context.SaveChangesAsync();

        var creatorPlayer = new GamePlayer { GameId = game.Id, UserId = creator.Id };
        var friendPlayer = new GamePlayer { GameId = game.Id, UserId = friend.Id };

        _context.GamePlayers.AddRange(creatorPlayer, friendPlayer);

        var gameQuestions = new List<GameQuestion>();
        for (int i = 0; i < questions.Count; i++)
        {
            var q = questions[i];
            var shuffled = q.GetShuffledAnswers();
            var gq = new GameQuestion
            {
                GameId = game.Id,
                QuestionId = q.Id,
                OrderIndex = i,
                ShuffledAnswersJson = JsonSerializer.Serialize(shuffled)
            };
            gameQuestions.Add(gq);
            _context.GameQuestions.Add(gq);
        }

        await _context.SaveChangesAsync();

        var session = new GameSession
        {
            GameId = game.Id,
            GameGuid = game.GameGuid,
            Status = GameStatus.WaitingForPlayers,
            Type = GameType.FriendGame,
            CategoryId = categoryId,
            CategoryName = categoryName,
            QuestionTimeSeconds = settings.QuestionTimeSeconds,
            CreatedAt = DateTime.UtcNow,
            Questions = gameQuestions.Select((gq, idx) =>
            {
                var q = questions[idx];
                var answers = JsonSerializer.Deserialize<string[]>(gq.ShuffledAnswersJson)!;
                return new GameSessionQuestion
                {
                    QuestionId = q.Id,
                    GameQuestionId = gq.Id,
                    Text = q.Text,
                    Answers = answers,
                    CorrectAnswer = q.CorrectAnswer,
                    CorrectAnswerIndex = Array.IndexOf(answers, q.CorrectAnswer),
                    Explanation = q.Explanation,
                    ImageUrl = q.ImageUrl
                };
            }).ToList()
        };

        session.Players[creatorTelegramId] = new PlayerSession
        {
            UserId = creator.Id,
            TelegramId = creatorTelegramId,
            Username = creator.Username,
            FirstName = creator.FirstName,
            LastName = creator.LastName,
            CountryCode = creator.CountryCode,
            GamePlayerId = creatorPlayer.Id
        };

        session.Players[friendTelegramId] = new PlayerSession
        {
            UserId = friend.Id,
            TelegramId = friendTelegramId,
            Username = friend.Username,
            FirstName = friend.FirstName,
            LastName = friend.LastName,
            CountryCode = friend.CountryCode,
            GamePlayerId = friendPlayer.Id
        };

        _sessionStore.AddSession(session);

        return session;
    }

    public async Task<GameSession?> JoinGameAsync(int gameId, long telegramId)
    {
        var session = _sessionStore.GetSession(gameId);
        if (session == null || session.Status != GameStatus.WaitingForPlayers)
            return null;

        if (session.Players.ContainsKey(telegramId))
            return session;

        if (session.Players.Count >= 2)
            return null;

        var user = await _userService.GetByTelegramIdAsync(telegramId);
        if (user == null) return null;

        var gamePlayer = new GamePlayer
        {
            GameId = gameId,
            UserId = user.Id,
            IsReady = false
        };
        _context.GamePlayers.Add(gamePlayer);
        await _context.SaveChangesAsync();

        var playerSession = new PlayerSession
        {
            UserId = user.Id,
            TelegramId = telegramId,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            CountryCode = user.CountryCode,
            GamePlayerId = gamePlayer.Id,
            IsReady = false
        };

        _sessionStore.AddPlayerToSession(gameId, playerSession);

        if (session.Players.Count == 2)
        {
            session.Status = GameStatus.WaitingForReady;
            var game = await _context.Games.FindAsync(gameId);
            if (game != null)
            {
                game.Status = GameStatus.WaitingForReady;
                await _context.SaveChangesAsync();
            }
        }

        return session;
    }

    public async Task<bool> SetPlayerReadyAsync(int gameId, long telegramId)
    {
        var session = _sessionStore.GetSession(gameId);
        if (session == null) return false;

        if (!session.Players.TryGetValue(telegramId, out var player))
            return false;

        player.IsReady = true;

        var gamePlayer = await _context.GamePlayers.FindAsync(player.GamePlayerId);
        if (gamePlayer != null)
        {
            gamePlayer.IsReady = true;
            await _context.SaveChangesAsync();
        }

        if (session.Players.Count == 2 && session.Players.Values.All(p => p.IsReady))
        {
            session.Status = GameStatus.InProgress;
            session.CurrentQuestionIndex = 0;
            session.QuestionStartedAt = DateTime.UtcNow;

            var game = await _context.Games.FindAsync(gameId);
            if (game != null)
            {
                game.Status = GameStatus.InProgress;
                game.StartedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        return true;
    }

    public Task<GameSession?> GetActiveGameAsync(long telegramId)
    {
        return Task.FromResult(_sessionStore.GetSessionByPlayer(telegramId));
    }

    public Task<GameSession?> GetGameByIdAsync(int gameId)
    {
        return Task.FromResult(_sessionStore.GetSession(gameId));
    }

    public Task<GameSessionQuestion?> GetCurrentQuestionAsync(int gameId)
    {
        var session = _sessionStore.GetSession(gameId);
        if (session == null || session.CurrentQuestionIndex >= session.Questions.Count)
            return Task.FromResult<GameSessionQuestion?>(null);

        return Task.FromResult<GameSessionQuestion?>(session.Questions[session.CurrentQuestionIndex]);
    }

    public async Task<AnswerResult> SubmitAnswerAsync(int gameId, long telegramId, int answerIndex)
    {
        var session = _sessionStore.GetSession(gameId);
        if (session == null || session.Status != GameStatus.InProgress)
            throw new InvalidOperationException("Game not in progress");

        if (!session.Players.TryGetValue(telegramId, out var player))
            throw new InvalidOperationException("Player not in game");

        var question = session.Questions[session.CurrentQuestionIndex];
        var timeMs = (long)(DateTime.UtcNow - session.QuestionStartedAt!.Value).TotalMilliseconds;
        var isCorrect = answerIndex == question.CorrectAnswerIndex;

        if (isCorrect)
        {
            player.CorrectAnswers++;
        }
        player.TotalTimeMs += timeMs;
        player.CurrentQuestionIndex = session.CurrentQuestionIndex + 1;
        player.LastAnswerTime = DateTime.UtcNow;

        var gameAnswer = new GameAnswer
        {
            GamePlayerId = player.GamePlayerId,
            GameQuestionId = question.GameQuestionId,
            SelectedAnswer = question.Answers[answerIndex],
            IsCorrect = isCorrect,
            TimeMs = timeMs,
            AnsweredAt = DateTime.UtcNow
        };
        _context.GameAnswers.Add(gameAnswer);

        var dbPlayer = await _context.GamePlayers.FindAsync(player.GamePlayerId);
        if (dbPlayer != null)
        {
            dbPlayer.CorrectAnswers = player.CorrectAnswers;
            dbPlayer.TotalTimeMs = player.TotalTimeMs;
            dbPlayer.CurrentQuestionIndex = player.CurrentQuestionIndex;
        }

        await _context.SaveChangesAsync();

        var opponent = session.Players.Values.FirstOrDefault(p => p.TelegramId != telegramId);
        var bothAnswered = opponent != null && opponent.CurrentQuestionIndex > session.CurrentQuestionIndex;

        OpponentAnswerInfo? opponentInfo = null;
        if (bothAnswered && opponent != null)
        {
            var opponentAnswer = await _context.GameAnswers
                .FirstOrDefaultAsync(a =>
                    a.GamePlayerId == opponent.GamePlayerId &&
                    a.GameQuestionId == question.GameQuestionId);

            if (opponentAnswer != null)
            {
                opponentInfo = new OpponentAnswerInfo
                {
                    IsCorrect = opponentAnswer.IsCorrect,
                    TimeMs = opponentAnswer.TimeMs
                };
            }
        }

        return new AnswerResult
        {
            IsCorrect = isCorrect,
            CorrectAnswer = question.CorrectAnswer,
            TimeMs = timeMs,
            BothAnswered = bothAnswered,
            OpponentAnswer = opponentInfo
        };
    }

    public async Task<bool> MoveToNextQuestionAsync(int gameId)
    {
        var session = _sessionStore.GetSession(gameId);
        if (session == null) return false;

        session.CurrentQuestionIndex++;
        session.QuestionStartedAt = DateTime.UtcNow;

        return session.CurrentQuestionIndex < session.Questions.Count;
    }

    public async Task<GameResult?> FinishGameAsync(int gameId)
    {
        var session = _sessionStore.GetSession(gameId);
        if (session == null) return null;

        var players = session.Players.Values.ToList();
        if (players.Count != 2) return null;

        var p1 = players[0];
        var p2 = players[1];

        int? winnerId = null;
        long? winnerTelegramId = null;
        bool isDraw = false;
        string winReason;

        if (p1.CorrectAnswers > p2.CorrectAnswers)
        {
            winnerId = p1.UserId;
            winnerTelegramId = p1.TelegramId;
            winReason = "по количеству правильных ответов";
        }
        else if (p2.CorrectAnswers > p1.CorrectAnswers)
        {
            winnerId = p2.UserId;
            winnerTelegramId = p2.TelegramId;
            winReason = "по количеству правильных ответов";
        }
        else if (p1.TotalTimeMs < p2.TotalTimeMs)
        {
            winnerId = p1.UserId;
            winnerTelegramId = p1.TelegramId;
            winReason = "по времени";
        }
        else if (p2.TotalTimeMs < p1.TotalTimeMs)
        {
            winnerId = p2.UserId;
            winnerTelegramId = p2.TelegramId;
            winReason = "по времени";
        }
        else
        {
            isDraw = true;
            winReason = "ничья";
        }

        var game = await _context.Games.FindAsync(gameId);
        if (game != null)
        {
            game.Status = GameStatus.Finished;
            game.FinishedAt = DateTime.UtcNow;
            game.WinnerId = winnerId;
            await _context.SaveChangesAsync();
        }

        if (winnerId.HasValue)
        {
            var winner = await _context.Users.FindAsync(winnerId.Value);
            if (winner != null)
            {
                winner.GamesWon++;
            }
        }

        foreach (var p in players)
        {
            var user = await _context.Users.FindAsync(p.UserId);
            if (user != null)
            {
                user.GamesPlayed++;
                user.TotalCorrectAnswers += p.CorrectAnswers;
            }
        }

        await _context.SaveChangesAsync();

        _sessionStore.RemoveSession(gameId);

        return new GameResult
        {
            GameId = gameId,
            Player1 = new PlayerResult
            {
                TelegramId = p1.TelegramId,
                Username = p1.Username,
                FirstName = p1.FirstName,
                LastName = p1.LastName,
                CountryCode = p1.CountryCode,
                CorrectAnswers = p1.CorrectAnswers,
                TotalTime = TimeSpan.FromMilliseconds(p1.TotalTimeMs)
            },
            Player2 = new PlayerResult
            {
                TelegramId = p2.TelegramId,
                Username = p2.Username,
                FirstName = p2.FirstName,
                LastName = p2.LastName,
                CountryCode = p2.CountryCode,
                CorrectAnswers = p2.CorrectAnswers,
                TotalTime = TimeSpan.FromMilliseconds(p2.TotalTimeMs)
            },
            WinnerTelegramId = winnerTelegramId,
            IsDraw = isDraw,
            WinReason = winReason
        };
    }

    public async Task CancelGameAsync(int gameId)
    {
        var game = await _context.Games.FindAsync(gameId);
        if (game != null)
        {
            game.Status = GameStatus.Cancelled;
            await _context.SaveChangesAsync();
        }

        _sessionStore.RemoveSession(gameId);
    }

    public async Task<UserStats> GetUserStatsAsync(long telegramId)
    {
        var user = await _userService.GetByTelegramIdAsync(telegramId);
        if (user == null)
        {
            return new UserStats();
        }

        return new UserStats
        {
            GamesPlayed = user.GamesPlayed,
            GamesWon = user.GamesWon,
            TotalCorrectAnswers = user.TotalCorrectAnswers
        };
    }

    public async Task<List<LeaderboardEntry>> GetLeaderboardAsync(int count = 10)
    {
        var topPlayers = await _context.Users
            .Where(u => u.GamesPlayed > 0)
            .OrderByDescending(u => u.GamesWon)
            .ThenByDescending(u => u.GamesPlayed > 0 ? (double)u.GamesWon / u.GamesPlayed : 0)
            .ThenBy(u => u.GamesPlayed)
            .Take(count)
            .ToListAsync();

        return topPlayers.Select((u, index) => new LeaderboardEntry
        {
            Rank = index + 1,
            TelegramId = u.TelegramId,
            Username = u.Username,
            FirstName = u.FirstName,
            GamesPlayed = u.GamesPlayed,
            GamesWon = u.GamesWon,
            TotalCorrectAnswers = u.TotalCorrectAnswers
        }).ToList();
    }

    public async Task<(int Rank, UserStats Stats)?> GetUserRankAsync(long telegramId)
    {
        var user = await _userService.GetByTelegramIdAsync(telegramId);
        if (user == null || user.GamesPlayed == 0)
            return null;

        var rank = await _context.Users
            .CountAsync(u => u.GamesWon > user.GamesWon ||
                            (u.GamesWon == user.GamesWon && u.GamesPlayed < user.GamesPlayed));

        return (rank + 1, new UserStats
        {
            GamesPlayed = user.GamesPlayed,
            GamesWon = user.GamesWon,
            TotalCorrectAnswers = user.TotalCorrectAnswers
        });
    }

    private async Task<(int QuestionTimeSeconds, int QuestionsCount)> GetGameSettingsAsync()
    {
        var timeSettings = await _context.GameSettings
            .FirstOrDefaultAsync(s => s.Key == "QuestionTimeSeconds");
        var countSettings = await _context.GameSettings
            .FirstOrDefaultAsync(s => s.Key == "QuestionsPerGame");

        return (
            int.TryParse(timeSettings?.Value, out var time) ? time : 15,
            int.TryParse(countSettings?.Value, out var count) ? count : 10
        );
    }
}
