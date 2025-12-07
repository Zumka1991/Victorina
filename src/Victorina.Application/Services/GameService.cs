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
        // Get the TranslationGroupId for the selected category
        Guid? categoryTranslationGroupId = null;
        if (categoryId.HasValue)
        {
            var category = await _context.Categories.FindAsync(categoryId.Value);
            categoryTranslationGroupId = category?.TranslationGroupId;
        }

        // Match by TranslationGroupId (cross-language) or by null (any category)
        var waitingSession = _sessionStore.GetWaitingSessions()
            .FirstOrDefault(s => !s.Players.ContainsKey(telegramId) &&
                                 s.CategoryTranslationGroupId == categoryTranslationGroupId);

        return waitingSession;
    }

    public async Task<GameSession> CreateQuickGameAsync(long telegramId, int? categoryId = null)
    {
        var user = await _userService.GetByTelegramIdAsync(telegramId);
        if (user == null) throw new InvalidOperationException("User not found");

        var settings = await GetGameSettingsAsync();
        // Load questions in player's language
        var questions = await _questionService.GetRandomQuestionsAsync(settings.QuestionsCount, categoryId, user.LanguageCode, user.Id);

        string? categoryName = null;
        Guid? categoryTranslationGroupId = null;
        if (categoryId.HasValue)
        {
            var category = await _context.Categories.FindAsync(categoryId.Value);
            categoryName = category?.Name;
            categoryTranslationGroupId = category?.TranslationGroupId;
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

        // Store GameQuestions in DB (question IDs without language-specific content)
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

        // Create player-specific questions list
        var playerQuestions = gameQuestions.Select((gq, idx) =>
        {
            var q = questions[idx];
            var answers = JsonSerializer.Deserialize<string[]>(gq.ShuffledAnswersJson)!;
            return new GameSessionQuestion
            {
                QuestionId = q.Id,
                GameQuestionId = gq.Id,
                TranslationGroupId = q.TranslationGroupId, // Save for finding translations
                Text = q.Text,
                Answers = answers,
                CorrectAnswer = q.CorrectAnswer,
                CorrectAnswerIndex = Array.IndexOf(answers, q.CorrectAnswer),
                Explanation = q.Explanation,
                ImageUrl = q.ImageUrl
            };
        }).ToList();

        var session = new GameSession
        {
            GameId = game.Id,
            GameGuid = game.GameGuid,
            Status = GameStatus.WaitingForPlayers,
            Type = GameType.QuickGame,
            CategoryId = categoryId,
            CategoryTranslationGroupId = categoryTranslationGroupId,
            CategoryName = categoryName,
            LanguageCode = user.LanguageCode,
            QuestionTimeSeconds = settings.QuestionTimeSeconds,
            CreatedAt = DateTime.UtcNow,
            Questions = playerQuestions // Keep for backwards compatibility, but player-specific questions are in PlayerSession
        };

        session.Players[telegramId] = new PlayerSession
        {
            UserId = user.Id,
            TelegramId = telegramId,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            CountryCode = user.CountryCode,
            LanguageCode = user.LanguageCode,
            GamePlayerId = gamePlayer.Id,
            IsReady = false,
            Questions = playerQuestions // Player's questions in their language
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

        string? categoryName = null;
        Guid? categoryTranslationGroupId = null;
        if (categoryId.HasValue)
        {
            var category = await _context.Categories.FindAsync(categoryId.Value);
            categoryName = category?.Name;
            categoryTranslationGroupId = category?.TranslationGroupId;
        }

        // Load questions for creator first
        var creatorQuestions = (await _questionService.GetRandomQuestionsAsync(
            settings.QuestionsCount, categoryId, creator.LanguageCode, creator.Id)).ToList();

        List<Question> friendQuestions;

        if (creator.LanguageCode == friend.LanguageCode)
        {
            // Same language - use same questions
            friendQuestions = creatorQuestions;
        }
        else
        {
            // Different languages - find translations of creator's questions
            var translationGroupIds = creatorQuestions
                .Where(q => q.TranslationGroupId.HasValue)
                .Select(q => q.TranslationGroupId!.Value)
                .ToList();

            if (translationGroupIds.Count > 0)
            {
                var translatedQuestions = await _questionService.GetQuestionsByTranslationGroupIdsAsync(
                    translationGroupIds, friend.LanguageCode);

                if (translatedQuestions.Count > 0)
                {
                    // Match translations to creator's questions by TranslationGroupId
                    var translatedByGroup = translatedQuestions.ToDictionary(q => q.TranslationGroupId!.Value);
                    friendQuestions = creatorQuestions.Select(cq =>
                    {
                        if (cq.TranslationGroupId.HasValue &&
                            translatedByGroup.TryGetValue(cq.TranslationGroupId.Value, out var translated))
                        {
                            return translated;
                        }
                        // No translation - use original
                        return cq;
                    }).ToList();
                }
                else
                {
                    // No translations found - use creator's questions
                    friendQuestions = creatorQuestions;
                }
            }
            else
            {
                // No TranslationGroupIds - use creator's questions
                friendQuestions = creatorQuestions;
            }
        }

        var game = new Game
        {
            Type = GameType.FriendGame,
            Status = GameStatus.WaitingForPlayers,
            CategoryId = categoryId,
            QuestionTimeSeconds = settings.QuestionTimeSeconds,
            TotalQuestions = creatorQuestions.Count,
            CreatedAt = DateTime.UtcNow
        };

        _context.Games.Add(game);
        await _context.SaveChangesAsync();

        var creatorPlayer = new GamePlayer { GameId = game.Id, UserId = creator.Id };
        var friendPlayer = new GamePlayer { GameId = game.Id, UserId = friend.Id };

        _context.GamePlayers.AddRange(creatorPlayer, friendPlayer);

        // Store GameQuestions using creator's questions for DB reference
        var gameQuestions = new List<GameQuestion>();
        for (int i = 0; i < creatorQuestions.Count; i++)
        {
            var q = creatorQuestions[i];
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

        // Create creator's questions list
        var creatorSessionQuestions = gameQuestions.Select((gq, idx) =>
        {
            var q = creatorQuestions[idx];
            var answers = JsonSerializer.Deserialize<string[]>(gq.ShuffledAnswersJson)!;
            return new GameSessionQuestion
            {
                QuestionId = q.Id,
                GameQuestionId = gq.Id,
                TranslationGroupId = q.TranslationGroupId,
                Text = q.Text,
                Answers = answers,
                CorrectAnswer = q.CorrectAnswer,
                CorrectAnswerIndex = Array.IndexOf(answers, q.CorrectAnswer),
                Explanation = q.Explanation,
                ImageUrl = q.ImageUrl
            };
        }).ToList();

        // Create friend's questions list
        var friendSessionQuestions = friendQuestions.Select((q, idx) =>
        {
            var answers = q.GetShuffledAnswers();
            var gameQuestion = gameQuestions.ElementAtOrDefault(idx);
            return new GameSessionQuestion
            {
                QuestionId = q.Id,
                GameQuestionId = gameQuestion?.Id ?? 0,
                TranslationGroupId = q.TranslationGroupId,
                Text = q.Text,
                Answers = answers,
                CorrectAnswer = q.CorrectAnswer,
                CorrectAnswerIndex = Array.IndexOf(answers, q.CorrectAnswer),
                Explanation = q.Explanation,
                ImageUrl = q.ImageUrl
            };
        }).ToList();

        var session = new GameSession
        {
            GameId = game.Id,
            GameGuid = game.GameGuid,
            Status = GameStatus.WaitingForPlayers,
            Type = GameType.FriendGame,
            CategoryId = categoryId,
            CategoryTranslationGroupId = categoryTranslationGroupId,
            CategoryName = categoryName,
            LanguageCode = creator.LanguageCode,
            QuestionTimeSeconds = settings.QuestionTimeSeconds,
            CreatedAt = DateTime.UtcNow,
            Questions = creatorSessionQuestions
        };

        session.Players[creatorTelegramId] = new PlayerSession
        {
            UserId = creator.Id,
            TelegramId = creatorTelegramId,
            Username = creator.Username,
            FirstName = creator.FirstName,
            LastName = creator.LastName,
            CountryCode = creator.CountryCode,
            LanguageCode = creator.LanguageCode,
            GamePlayerId = creatorPlayer.Id,
            Questions = creatorSessionQuestions
        };

        session.Players[friendTelegramId] = new PlayerSession
        {
            UserId = friend.Id,
            TelegramId = friendTelegramId,
            Username = friend.Username,
            FirstName = friend.FirstName,
            LastName = friend.LastName,
            CountryCode = friend.CountryCode,
            LanguageCode = friend.LanguageCode,
            GamePlayerId = friendPlayer.Id,
            Questions = friendSessionQuestions
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

        // Get the first player to determine their language
        var firstPlayer = session.Players.Values.First();
        var settings = await GetGameSettingsAsync();

        List<GameSessionQuestion> playerQuestions;

        // If both players have same language, use existing questions
        if (user.LanguageCode == firstPlayer.LanguageCode)
        {
            // Same language - create questions with same text but shuffled answers
            playerQuestions = firstPlayer.Questions.Select(q =>
            {
                var allAnswers = new List<string> { q.CorrectAnswer };
                allAnswers.AddRange(q.Answers.Where(a => a != q.CorrectAnswer));
                var answers = allAnswers.OrderBy(_ => Random.Shared.Next()).ToArray();
                return new GameSessionQuestion
                {
                    QuestionId = q.QuestionId,
                    GameQuestionId = q.GameQuestionId,
                    TranslationGroupId = q.TranslationGroupId,
                    Text = q.Text,
                    Answers = answers,
                    CorrectAnswer = q.CorrectAnswer,
                    CorrectAnswerIndex = Array.IndexOf(answers, q.CorrectAnswer),
                    Explanation = q.Explanation,
                    ImageUrl = q.ImageUrl
                };
            }).ToList();
        }
        else
        {
            // Different languages - find translations of the SAME questions first player has
            var translationGroupIds = firstPlayer.Questions
                .Where(q => q.TranslationGroupId.HasValue)
                .Select(q => q.TranslationGroupId!.Value)
                .ToList();

            if (translationGroupIds.Count > 0)
            {
                // Try to get translated versions of the same questions
                var translatedQuestions = await _questionService.GetQuestionsByTranslationGroupIdsAsync(
                    translationGroupIds, user.LanguageCode);

                if (translatedQuestions.Count > 0)
                {
                    // Build a lookup for translated questions by TranslationGroupId
                    var translatedByGroup = translatedQuestions.ToDictionary(q => q.TranslationGroupId!.Value);

                    // Create questions for second player, matching by TranslationGroupId
                    playerQuestions = firstPlayer.Questions.Select(originalQ =>
                    {
                        // Try to find translation
                        if (originalQ.TranslationGroupId.HasValue &&
                            translatedByGroup.TryGetValue(originalQ.TranslationGroupId.Value, out var translated))
                        {
                            var answers = translated.GetShuffledAnswers();
                            return new GameSessionQuestion
                            {
                                QuestionId = translated.Id,
                                GameQuestionId = originalQ.GameQuestionId,
                                TranslationGroupId = translated.TranslationGroupId,
                                Text = translated.Text,
                                Answers = answers,
                                CorrectAnswer = translated.CorrectAnswer,
                                CorrectAnswerIndex = Array.IndexOf(answers, translated.CorrectAnswer),
                                Explanation = translated.Explanation,
                                ImageUrl = translated.ImageUrl
                            };
                        }
                        else
                        {
                            // No translation found - use original question (player will see different language)
                            var answers = originalQ.Answers.OrderBy(_ => Random.Shared.Next()).ToArray();
                            return new GameSessionQuestion
                            {
                                QuestionId = originalQ.QuestionId,
                                GameQuestionId = originalQ.GameQuestionId,
                                TranslationGroupId = originalQ.TranslationGroupId,
                                Text = originalQ.Text,
                                Answers = answers,
                                CorrectAnswer = originalQ.CorrectAnswer,
                                CorrectAnswerIndex = Array.IndexOf(answers, originalQ.CorrectAnswer),
                                Explanation = originalQ.Explanation,
                                ImageUrl = originalQ.ImageUrl
                            };
                        }
                    }).ToList();
                }
                else
                {
                    // No translations found at all - use original questions
                    playerQuestions = firstPlayer.Questions.Select(q =>
                    {
                        var answers = q.Answers.OrderBy(_ => Random.Shared.Next()).ToArray();
                        return new GameSessionQuestion
                        {
                            QuestionId = q.QuestionId,
                            GameQuestionId = q.GameQuestionId,
                            TranslationGroupId = q.TranslationGroupId,
                            Text = q.Text,
                            Answers = answers,
                            CorrectAnswer = q.CorrectAnswer,
                            CorrectAnswerIndex = Array.IndexOf(answers, q.CorrectAnswer),
                            Explanation = q.Explanation,
                            ImageUrl = q.ImageUrl
                        };
                    }).ToList();
                }
            }
            else
            {
                // No TranslationGroupIds - questions don't have translations, use original
                playerQuestions = firstPlayer.Questions.Select(q =>
                {
                    var answers = q.Answers.OrderBy(_ => Random.Shared.Next()).ToArray();
                    return new GameSessionQuestion
                    {
                        QuestionId = q.QuestionId,
                        GameQuestionId = q.GameQuestionId,
                        TranslationGroupId = q.TranslationGroupId,
                        Text = q.Text,
                        Answers = answers,
                        CorrectAnswer = q.CorrectAnswer,
                        CorrectAnswerIndex = Array.IndexOf(answers, q.CorrectAnswer),
                        Explanation = q.Explanation,
                        ImageUrl = q.ImageUrl
                    };
                }).ToList();
            }
        }

        var playerSession = new PlayerSession
        {
            UserId = user.Id,
            TelegramId = telegramId,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            CountryCode = user.CountryCode,
            LanguageCode = user.LanguageCode,
            GamePlayerId = gamePlayer.Id,
            IsReady = false,
            Questions = playerQuestions
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

            // Record shown questions for both players to avoid repetition in future games
            await RecordShownQuestionsAsync(session);
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

    public Task<GameSessionQuestion?> GetCurrentQuestionForPlayerAsync(int gameId, long telegramId)
    {
        var session = _sessionStore.GetSession(gameId);
        if (session == null || session.CurrentQuestionIndex >= session.Questions.Count)
            return Task.FromResult<GameSessionQuestion?>(null);

        // Get player-specific question in their language
        if (session.Players.TryGetValue(telegramId, out var player) && player.Questions.Count > session.CurrentQuestionIndex)
        {
            return Task.FromResult<GameSessionQuestion?>(player.Questions[session.CurrentQuestionIndex]);
        }

        // Fallback to session questions
        return Task.FromResult<GameSessionQuestion?>(session.Questions[session.CurrentQuestionIndex]);
    }

    public async Task<AnswerResult> SubmitAnswerAsync(int gameId, long telegramId, int answerIndex)
    {
        var session = _sessionStore.GetSession(gameId);
        if (session == null || session.Status != GameStatus.InProgress)
            throw new InvalidOperationException("Game not in progress");

        if (!session.Players.TryGetValue(telegramId, out var player))
            throw new InvalidOperationException("Player not in game");

        // Use player's question in their language
        var question = player.Questions.Count > session.CurrentQuestionIndex
            ? player.Questions[session.CurrentQuestionIndex]
            : session.Questions[session.CurrentQuestionIndex];
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

    public async Task<List<TimeoutInfo>> CheckAndHandleTimeoutsAsync()
    {
        var timeouts = new List<TimeoutInfo>();
        var activeSessions = _sessionStore.GetActiveGameSessions().ToList();

        foreach (var session in activeSessions)
        {
            if (session.QuestionStartedAt == null) continue;

            var elapsedSeconds = (DateTime.UtcNow - session.QuestionStartedAt.Value).TotalSeconds;

            // Add 2 second buffer for network latency
            if (elapsedSeconds <= session.QuestionTimeSeconds + 2) continue;

            var question = session.Questions[session.CurrentQuestionIndex];
            var playersWhoNeedTimeout = session.Players.Values
                .Where(p => p.CurrentQuestionIndex == session.CurrentQuestionIndex)
                .ToList();

            if (playersWhoNeedTimeout.Count == 0) continue;

            var bothTimedOut = playersWhoNeedTimeout.Count == 2;

            foreach (var player in playersWhoNeedTimeout)
            {
                // Record timeout as wrong answer with max time
                var timeMs = (long)(session.QuestionTimeSeconds * 1000) + 2000;
                player.TotalTimeMs += timeMs;
                player.CurrentQuestionIndex = session.CurrentQuestionIndex + 1;
                player.LastAnswerTime = DateTime.UtcNow;

                var gameAnswer = new GameAnswer
                {
                    GamePlayerId = player.GamePlayerId,
                    GameQuestionId = question.GameQuestionId,
                    SelectedAnswer = "TIMEOUT",
                    IsCorrect = false,
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
            }

            await _context.SaveChangesAsync();

            // Check if game should finish or move to next question
            var allAnswered = session.Players.Values.All(p => p.CurrentQuestionIndex > session.CurrentQuestionIndex);
            var isLastQuestion = session.CurrentQuestionIndex + 1 >= session.Questions.Count;

            GameResult? gameResult = null;
            if (allAnswered && isLastQuestion)
            {
                gameResult = await FinishGameAsync(session.GameId);
            }
            else if (allAnswered)
            {
                await MoveToNextQuestionAsync(session.GameId);
            }

            // Игроки, которые уже ответили и ждут следующего вопроса
            var playersWhoAnswered = session.Players.Values
                .Where(p => !playersWhoNeedTimeout.Any(t => t.TelegramId == p.TelegramId))
                .Select(p => p.TelegramId)
                .ToList();

            foreach (var player in playersWhoNeedTimeout)
            {
                timeouts.Add(new TimeoutInfo
                {
                    GameId = session.GameId,
                    TelegramId = player.TelegramId,
                    CorrectAnswer = question.CorrectAnswer,
                    BothTimedOut = bothTimedOut,
                    IsLastQuestion = isLastQuestion && allAnswered,
                    GameResult = gameResult,
                    PlayersWaitingForNextQuestion = playersWhoAnswered
                });
            }
        }

        return timeouts;
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

    private async Task RecordShownQuestionsAsync(GameSession session)
    {
        var now = DateTime.UtcNow;
        var historyEntries = new List<UserQuestionHistory>();

        foreach (var player in session.Players.Values)
        {
            // Get unique TranslationGroupIds from player's questions
            var translationGroupIds = player.Questions
                .Where(q => q.TranslationGroupId.HasValue)
                .Select(q => q.TranslationGroupId!.Value)
                .Distinct()
                .ToList();

            foreach (var groupId in translationGroupIds)
            {
                historyEntries.Add(new UserQuestionHistory
                {
                    UserId = player.UserId,
                    QuestionTranslationGroupId = groupId,
                    ShownAt = now
                });
            }
        }

        if (historyEntries.Count > 0)
        {
            _context.UserQuestionHistories.AddRange(historyEntries);
            await _context.SaveChangesAsync();
        }
    }
}
