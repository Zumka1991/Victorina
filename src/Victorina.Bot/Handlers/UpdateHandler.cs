using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Victorina.Application.Interfaces;
using Victorina.Bot.Constants;
using Victorina.Bot.Services;
using Victorina.Domain.Enums;

namespace Victorina.Bot.Handlers;

public class UpdateHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly IServiceProvider _serviceProvider;
    private readonly KeyboardService _keyboard;
    private readonly UserStateService _userState;
    private readonly ILogger<UpdateHandler> _logger;

    public UpdateHandler(
        ITelegramBotClient bot,
        IServiceProvider serviceProvider,
        KeyboardService keyboard,
        UserStateService userState,
        ILogger<UpdateHandler> logger)
    {
        _bot = bot;
        _serviceProvider = serviceProvider;
        _keyboard = keyboard;
        _userState = userState;
        _logger = logger;
    }

    public async Task HandleUpdateAsync(Update update, CancellationToken ct)
    {
        try
        {
            var handler = update.Type switch
            {
                UpdateType.Message => HandleMessageAsync(update.Message!, ct),
                UpdateType.CallbackQuery => HandleCallbackAsync(update.CallbackQuery!, ct),
                _ => Task.CompletedTask
            };

            await handler;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling update");
        }
    }

    private async Task HandleMessageAsync(Message message, CancellationToken ct)
    {
        if (message.Text is not { } text)
            return;

        using var scope = _serviceProvider.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var gameService = scope.ServiceProvider.GetRequiredService<IGameService>();
        var friendshipService = scope.ServiceProvider.GetRequiredService<IFriendshipService>();

        var telegramUser = message.From!;
        var user = await userService.GetOrCreateUserAsync(
            telegramUser.Id,
            telegramUser.Username,
            telegramUser.FirstName,
            telegramUser.LastName);

        var chatId = message.Chat.Id;
        var state = _userState.GetState(telegramUser.Id);

        // Обработка состояния поиска друга
        if (state == UserState.WaitingForFriendSearch)
        {
            if (text == "🔙 Назад" || text == "❌ Отмена")
            {
                _userState.ClearState(telegramUser.Id);
                await SendFriendsMenu(chatId, ct);
                return;
            }
            await HandleFriendSearchAsync(chatId, telegramUser.Id, user.Id, text, ct);
            return;
        }

        // Обработка Reply-кнопок
        switch (text)
        {
            case "/start":
            case "/menu":
            case "🔙 Назад":
                await SendMainMenu(chatId, ct);
                break;

            case "🎮 Играть":
                await SendPlayMenu(chatId, ct);
                break;

            case "⚡ Быстрая игра":
                await HandleQuickGameReplyAsync(chatId, telegramUser.Id, gameService, ct);
                break;

            case "👤 Играть с другом":
                await HandlePlayWithFriendReplyAsync(chatId, user.Id, friendshipService, ct);
                break;

            case "📊 Статистика":
                await HandleStatisticsReplyAsync(chatId, telegramUser.Id, gameService, ct);
                break;

            case "👥 Друзья":
                await SendFriendsMenu(chatId, ct);
                break;

            case "📋 Мои друзья":
                await HandleFriendsListReplyAsync(chatId, user.Id, friendshipService, ct);
                break;

            case "➕ Добавить друга":
                _userState.SetState(telegramUser.Id, UserState.WaitingForFriendSearch);
                await _bot.SendMessage(chatId,
                    "🔍 Введите @username или номер телефона друга:",
                    replyMarkup: _keyboard.GetCancelReplyKeyboard(),
                    cancellationToken: ct);
                break;

            case "📩 Запросы":
                await HandleFriendRequestsReplyAsync(chatId, user.Id, friendshipService, ct);
                break;

            case "❌ Отмена":
            case "❌ Покинуть игру":
                var activeGame = await gameService.GetActiveGameAsync(telegramUser.Id);
                if (activeGame != null)
                {
                    await gameService.CancelGameAsync(activeGame.GameId);
                    foreach (var player in activeGame.Players.Values.Where(p => p.TelegramId != telegramUser.Id))
                    {
                        await _bot.SendMessage(player.TelegramId,
                            "😔 Соперник покинул игру.",
                            replyMarkup: _keyboard.GetMainMenuReplyKeyboard(),
                            cancellationToken: ct);
                    }
                }
                await SendMainMenu(chatId, ct);
                break;

            case "/help":
            case "❓ Помощь":
                await _bot.SendMessage(chatId,
                    "🎯 *Викторина* — игра, где вы соревнуетесь с друзьями!\n\n" +
                    "🎮 *Как играть:*\n" +
                    "1. Нажмите «Играть»\n" +
                    "2. Выберите быструю игру или играйте с другом\n" +
                    "3. Отвечайте на вопросы быстрее соперника!\n\n" +
                    "🏆 Побеждает тот, кто даст больше правильных ответов. При равенстве — кто быстрее!",
                    parseMode: ParseMode.Markdown,
                    replyMarkup: _keyboard.GetMainMenuReplyKeyboard(),
                    cancellationToken: ct);
                break;

            default:
                if (text.StartsWith("/"))
                {
                    await SendMainMenu(chatId, ct);
                }
                break;
        }
    }

    private async Task SendMainMenu(long chatId, CancellationToken ct)
    {
        await _bot.SendMessage(chatId,
            "🎯 *Викторина*\n\nВыберите действие:",
            parseMode: ParseMode.Markdown,
            replyMarkup: _keyboard.GetMainMenuReplyKeyboard(),
            cancellationToken: ct);
    }

    private async Task SendPlayMenu(long chatId, CancellationToken ct)
    {
        await _bot.SendMessage(chatId,
            "🎮 *Выберите режим игры:*",
            parseMode: ParseMode.Markdown,
            replyMarkup: _keyboard.GetPlayMenuReplyKeyboard(),
            cancellationToken: ct);
    }

    private async Task SendFriendsMenu(long chatId, CancellationToken ct)
    {
        await _bot.SendMessage(chatId,
            "👥 *Друзья*\n\nВыберите действие:",
            parseMode: ParseMode.Markdown,
            replyMarkup: _keyboard.GetFriendsMenuReplyKeyboard(),
            cancellationToken: ct);
    }

    private async Task HandleQuickGameReplyAsync(long chatId, long telegramId,
        IGameService gameService, CancellationToken ct)
    {
        var activeGame = await gameService.GetActiveGameAsync(telegramId);
        if (activeGame != null)
        {
            await _bot.SendMessage(chatId,
                "⚠️ У вас уже есть активная игра!",
                replyMarkup: _keyboard.GetGameReplyKeyboard(),
                cancellationToken: ct);
            return;
        }

        var session = await gameService.FindQuickGameAsync(telegramId);

        if (session != null)
        {
            session = await gameService.JoinGameAsync(session.GameId, telegramId);
            var opponent = session!.Players.Values.First(p => p.TelegramId != telegramId);

            await _bot.SendMessage(chatId,
                $"🎮 *Соперник найден!*\n\n👤 Вы vs 👤 {opponent.Username}\n\nНажмите «Готов» чтобы начать!",
                parseMode: ParseMode.Markdown,
                replyMarkup: _keyboard.GetReadyKeyboard(),
                cancellationToken: ct);

            await _bot.SendMessage(opponent.TelegramId,
                $"🎮 *Соперник найден!*\n\n👤 Вы vs 👤 Соперник\n\nНажмите «Готов» чтобы начать!",
                parseMode: ParseMode.Markdown,
                replyMarkup: _keyboard.GetReadyKeyboard(),
                cancellationToken: ct);
        }
        else
        {
            await gameService.CreateQuickGameAsync(telegramId);
            await _bot.SendMessage(chatId,
                "🔍 *Ищем соперника...*\n\nПодождите, пока кто-то присоединится.",
                parseMode: ParseMode.Markdown,
                replyMarkup: _keyboard.GetSearchingKeyboard(),
                cancellationToken: ct);
        }
    }

    private async Task HandleStatisticsReplyAsync(long chatId, long telegramId,
        IGameService gameService, CancellationToken ct)
    {
        var stats = await gameService.GetUserStatsAsync(telegramId);

        await _bot.SendMessage(chatId,
            $"📊 *Ваша статистика*\n\n" +
            $"🎮 Игр сыграно: *{stats.GamesPlayed}*\n" +
            $"🏆 Побед: *{stats.GamesWon}*\n" +
            $"📈 Процент побед: *{stats.WinRate:F1}%*\n" +
            $"✅ Правильных ответов: *{stats.TotalCorrectAnswers}*",
            parseMode: ParseMode.Markdown,
            replyMarkup: _keyboard.GetMainMenuReplyKeyboard(),
            cancellationToken: ct);
    }

    private async Task HandleFriendsListReplyAsync(long chatId, int userId,
        IFriendshipService friendshipService, CancellationToken ct)
    {
        var friends = await friendshipService.GetFriendsAsync(userId);

        if (friends.Count == 0)
        {
            await _bot.SendMessage(chatId,
                "😔 У вас пока нет друзей.\n\nНажмите «Добавить друга» чтобы найти игроков!",
                replyMarkup: _keyboard.GetFriendsMenuReplyKeyboard(),
                cancellationToken: ct);
        }
        else
        {
            await _bot.SendMessage(chatId,
                "👥 *Ваши друзья:*\n\nВыберите друга для игры:",
                parseMode: ParseMode.Markdown,
                replyMarkup: _keyboard.GetFriendsListKeyboard(friends),
                cancellationToken: ct);
        }
    }

    private async Task HandleFriendRequestsReplyAsync(long chatId, int userId,
        IFriendshipService friendshipService, CancellationToken ct)
    {
        var requests = await friendshipService.GetPendingRequestsAsync(userId);

        if (requests.Count == 0)
        {
            await _bot.SendMessage(chatId,
                "📭 Нет входящих запросов в друзья.",
                replyMarkup: _keyboard.GetFriendsMenuReplyKeyboard(),
                cancellationToken: ct);
        }
        else
        {
            await _bot.SendMessage(chatId,
                "📩 *Входящие запросы:*",
                parseMode: ParseMode.Markdown,
                replyMarkup: _keyboard.GetFriendRequestsKeyboard(requests),
                cancellationToken: ct);
        }
    }

    private async Task HandlePlayWithFriendReplyAsync(long chatId, int userId,
        IFriendshipService friendshipService, CancellationToken ct)
    {
        var friends = await friendshipService.GetFriendsAsync(userId);

        if (friends.Count == 0)
        {
            await _bot.SendMessage(chatId,
                "😔 У вас пока нет друзей.\n\nСначала добавьте друзей в разделе «Друзья»!",
                replyMarkup: _keyboard.GetPlayMenuReplyKeyboard(),
                cancellationToken: ct);
        }
        else
        {
            await _bot.SendMessage(chatId,
                "👤 *Выберите друга для игры:*",
                parseMode: ParseMode.Markdown,
                replyMarkup: _keyboard.GetFriendsListKeyboard(friends),
                cancellationToken: ct);
        }
    }

    private async Task HandleCallbackAsync(CallbackQuery callback, CancellationToken ct)
    {
        var chatId = callback.Message!.Chat.Id;
        var messageId = callback.Message.MessageId;
        var telegramId = callback.From.Id;
        var data = callback.Data ?? string.Empty;

        using var scope = _serviceProvider.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var gameService = scope.ServiceProvider.GetRequiredService<IGameService>();
        var friendshipService = scope.ServiceProvider.GetRequiredService<IFriendshipService>();

        var user = await userService.GetOrCreateUserAsync(
            callback.From.Id,
            callback.From.Username,
            callback.From.FirstName,
            callback.From.LastName);

        try
        {
            switch (data)
            {
                case CallbackData.QuickGame:
                    await HandleQuickGameReplyAsync(chatId, telegramId, gameService, ct);
                    break;

                case CallbackData.CheckGame:
                    await HandleCheckGameAsync(chatId, messageId, telegramId, gameService, ct);
                    break;

                case CallbackData.Ready:
                    await HandleReadyAsync(chatId, messageId, telegramId, gameService, ct);
                    break;

                case CallbackData.CheckOpponent:
                    await HandleCheckOpponentAsync(chatId, messageId, telegramId, gameService, ct);
                    break;

                case CallbackData.CancelGame:
                    await HandleCancelGameAsync(chatId, messageId, telegramId, gameService, ct);
                    break;

                case CallbackData.PlayWithFriend:
                    await HandlePlayWithFriendReplyAsync(chatId, user.Id, friendshipService, ct);
                    break;

                case CallbackData.Friends:
                    await SendFriendsMenu(chatId, ct);
                    break;

                case CallbackData.BackToMenu:
                    _userState.ClearState(telegramId);
                    await SendMainMenu(chatId, ct);
                    break;

                default:
                    if (data.StartsWith(CallbackData.Answer))
                    {
                        await HandleAnswerAsync(chatId, messageId, telegramId, data, gameService, ct);
                    }
                    else if (data.StartsWith(CallbackData.AcceptFriend))
                    {
                        await HandleAcceptFriendAsync(chatId, messageId, user.Id, data, friendshipService, ct);
                    }
                    else if (data.StartsWith(CallbackData.RejectFriend))
                    {
                        await HandleRejectFriendAsync(chatId, messageId, user.Id, data, friendshipService, ct);
                    }
                    else if (data.StartsWith(CallbackData.InviteFriend))
                    {
                        await HandleInviteFriendAsync(chatId, messageId, telegramId, user.Id, data,
                            gameService, userService, ct);
                    }
                    break;
            }

            await _bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling callback {Data}", data);
            await _bot.AnswerCallbackQuery(callback.Id, "Произошла ошибка", cancellationToken: ct);
        }
    }

    private async Task HandleCheckGameAsync(long chatId, int messageId, long telegramId,
        IGameService gameService, CancellationToken ct)
    {
        var session = await gameService.GetActiveGameAsync(telegramId);

        if (session == null)
        {
            await _bot.SendMessage(chatId,
                "❌ Игра не найдена.",
                replyMarkup: _keyboard.GetMainMenuReplyKeyboard(),
                cancellationToken: ct);
            return;
        }

        if (session.Status == GameStatus.WaitingForPlayers)
        {
            await _bot.SendMessage(chatId,
                "🔍 Пока ищем соперника... Подождите немного.",
                cancellationToken: ct);
        }
        else if (session.Status == GameStatus.WaitingForReady)
        {
            var opponent = session.Players.Values.First(p => p.TelegramId != telegramId);
            await _bot.SendMessage(chatId,
                $"🎮 Соперник найден: {opponent.Username}\n\nНажмите «Готов»!",
                replyMarkup: _keyboard.GetReadyKeyboard(),
                cancellationToken: ct);
        }
    }

    private async Task HandleReadyAsync(long chatId, int messageId, long telegramId,
        IGameService gameService, CancellationToken ct)
    {
        var session = await gameService.GetActiveGameAsync(telegramId);
        if (session == null) return;

        await gameService.SetPlayerReadyAsync(session.GameId, telegramId);
        session = await gameService.GetActiveGameAsync(telegramId);
        if (session == null) return;

        if (session.Status == GameStatus.InProgress)
        {
            var question = session.Questions[0];

            foreach (var player in session.Players.Values)
            {
                await _bot.SendMessage(player.TelegramId,
                    "🚀 *Игра начинается!*",
                    parseMode: ParseMode.Markdown,
                    replyMarkup: _keyboard.GetGameReplyKeyboard(),
                    cancellationToken: ct);

                await Task.Delay(1000, ct);

                await _bot.SendMessage(player.TelegramId,
                    $"❓ *Вопрос 1/{session.Questions.Count}*\n\n{question.Text}",
                    parseMode: ParseMode.Markdown,
                    replyMarkup: _keyboard.GetQuestionKeyboard(question.Answers),
                    cancellationToken: ct);
            }
        }
        else
        {
            await _bot.SendMessage(chatId,
                "✅ Вы готовы! Ожидаем соперника...",
                replyMarkup: _keyboard.GetWaitingOpponentKeyboard(),
                cancellationToken: ct);
        }
    }

    private async Task HandleAnswerAsync(long chatId, int messageId, long telegramId, string data,
        IGameService gameService, CancellationToken ct)
    {
        var answerIndex = int.Parse(data.Replace(CallbackData.Answer, ""));

        var session = await gameService.GetActiveGameAsync(telegramId);
        if (session == null || session.Status != GameStatus.InProgress) return;

        var result = await gameService.SubmitAnswerAsync(session.GameId, telegramId, answerIndex);

        var emoji = result.IsCorrect ? "✅" : "❌";
        await _bot.EditMessageText(chatId, messageId,
            $"{emoji} {(result.IsCorrect ? "Правильно!" : "Неверно!")}\n\n" +
            $"Правильный ответ: *{result.CorrectAnswer}*\n" +
            $"⏱ Ваше время: {result.TimeMs / 1000.0:F2} сек",
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);

        session = await gameService.GetActiveGameAsync(telegramId);
        if (session == null) return;

        var player = session.Players[telegramId];
        var opponent = session.Players.Values.First(p => p.TelegramId != telegramId);

        if (player.CurrentQuestionIndex == opponent.CurrentQuestionIndex)
        {
            if (session.CurrentQuestionIndex + 1 >= session.Questions.Count)
            {
                var gameResult = await gameService.FinishGameAsync(session.GameId);
                if (gameResult != null)
                {
                    await SendGameResultsAsync(gameResult, ct);
                }
            }
            else
            {
                await gameService.MoveToNextQuestionAsync(session.GameId);
                session = await gameService.GetGameByIdAsync(session.GameId);

                if (session != null)
                {
                    var nextQuestion = session.Questions[session.CurrentQuestionIndex];

                    foreach (var p in session.Players.Values)
                    {
                        await Task.Delay(1500, ct);
                        await _bot.SendMessage(p.TelegramId,
                            $"❓ *Вопрос {session.CurrentQuestionIndex + 1}/{session.Questions.Count}*\n\n{nextQuestion.Text}",
                            parseMode: ParseMode.Markdown,
                            replyMarkup: _keyboard.GetQuestionKeyboard(nextQuestion.Answers),
                            cancellationToken: ct);
                    }
                }
            }
        }
        else
        {
            await Task.Delay(500, ct);
            await _bot.SendMessage(chatId,
                "⏳ Ожидаем ответ соперника...",
                replyMarkup: _keyboard.GetWaitingOpponentKeyboard(),
                cancellationToken: ct);
        }
    }

    private async Task HandleCheckOpponentAsync(long chatId, int messageId, long telegramId,
        IGameService gameService, CancellationToken ct)
    {
        var session = await gameService.GetActiveGameAsync(telegramId);
        if (session == null) return;

        var player = session.Players[telegramId];
        var opponent = session.Players.Values.First(p => p.TelegramId != telegramId);

        if (player.CurrentQuestionIndex == opponent.CurrentQuestionIndex &&
            session.Status == GameStatus.InProgress)
        {
            if (session.CurrentQuestionIndex < session.Questions.Count)
            {
                var question = session.Questions[session.CurrentQuestionIndex];
                await _bot.SendMessage(chatId,
                    $"❓ *Вопрос {session.CurrentQuestionIndex + 1}/{session.Questions.Count}*\n\n{question.Text}",
                    parseMode: ParseMode.Markdown,
                    replyMarkup: _keyboard.GetQuestionKeyboard(question.Answers),
                    cancellationToken: ct);
            }
        }
        else
        {
            await _bot.SendMessage(chatId,
                "⏳ Соперник ещё отвечает...",
                cancellationToken: ct);
        }
    }

    private async Task SendGameResultsAsync(Application.Models.GameResult result, CancellationToken ct)
    {
        foreach (var playerResult in new[] { result.Player1, result.Player2 })
        {
            var isWinner = result.WinnerTelegramId == playerResult.TelegramId;
            var opponent = playerResult == result.Player1 ? result.Player2 : result.Player1;

            string emoji, title;
            if (result.IsDraw)
            {
                emoji = "🤝";
                title = "Ничья!";
            }
            else if (isWinner)
            {
                emoji = "🏆";
                title = "Вы победили!";
            }
            else
            {
                emoji = "😔";
                title = "Вы проиграли";
            }

            var message = $"{emoji} *{title}*\n\n" +
                         $"📊 *Ваш результат:*\n" +
                         $"✅ Правильных: {playerResult.CorrectAnswers}\n" +
                         $"⏱ Время: {playerResult.TotalTime.TotalSeconds:F2} сек\n\n" +
                         $"📊 *Соперник:*\n" +
                         $"✅ Правильных: {opponent.CorrectAnswers}\n" +
                         $"⏱ Время: {opponent.TotalTime.TotalSeconds:F2} сек";

            if (!result.IsDraw)
            {
                message += $"\n\n_{result.WinReason}_";
            }

            await _bot.SendMessage(playerResult.TelegramId,
                message,
                parseMode: ParseMode.Markdown,
                replyMarkup: _keyboard.GetMainMenuReplyKeyboard(),
                cancellationToken: ct);
        }
    }

    private async Task HandleCancelGameAsync(long chatId, int messageId, long telegramId,
        IGameService gameService, CancellationToken ct)
    {
        var session = await gameService.GetActiveGameAsync(telegramId);
        if (session != null)
        {
            await gameService.CancelGameAsync(session.GameId);

            foreach (var player in session.Players.Values.Where(p => p.TelegramId != telegramId))
            {
                await _bot.SendMessage(player.TelegramId,
                    "😔 Соперник отменил игру.",
                    replyMarkup: _keyboard.GetMainMenuReplyKeyboard(),
                    cancellationToken: ct);
            }
        }

        await _bot.SendMessage(chatId,
            "❌ Игра отменена.",
            replyMarkup: _keyboard.GetMainMenuReplyKeyboard(),
            cancellationToken: ct);
    }

    private async Task HandleFriendSearchAsync(long chatId, long telegramId, int userId,
        string searchText, CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var friendshipService = scope.ServiceProvider.GetRequiredService<IFriendshipService>();

        _userState.ClearState(telegramId);

        var searchQuery = searchText.TrimStart('@');
        var foundUser = await userService.FindByUsernameAsync(searchQuery)
                        ?? await userService.FindByPhoneAsync(searchQuery);

        if (foundUser == null || foundUser.Id == userId)
        {
            await _bot.SendMessage(chatId,
                "❌ Пользователь не найден.",
                replyMarkup: _keyboard.GetFriendsMenuReplyKeyboard(),
                cancellationToken: ct);
            return;
        }

        var areFriends = await friendshipService.AreFriendsAsync(userId, foundUser.Id);
        if (areFriends)
        {
            await _bot.SendMessage(chatId,
                "👥 Вы уже друзья!",
                replyMarkup: _keyboard.GetFriendsMenuReplyKeyboard(),
                cancellationToken: ct);
            return;
        }

        var request = await friendshipService.SendFriendRequestAsync(userId, foundUser.Id);
        if (request != null)
        {
            await _bot.SendMessage(chatId,
                "✅ Запрос в друзья отправлен!",
                replyMarkup: _keyboard.GetFriendsMenuReplyKeyboard(),
                cancellationToken: ct);

            await _bot.SendMessage(foundUser.TelegramId,
                "📩 У вас новый запрос в друзья!",
                replyMarkup: _keyboard.GetFriendsMenuReplyKeyboard(),
                cancellationToken: ct);
        }
        else
        {
            await _bot.SendMessage(chatId,
                "⚠️ Запрос уже существует.",
                replyMarkup: _keyboard.GetFriendsMenuReplyKeyboard(),
                cancellationToken: ct);
        }
    }

    private async Task HandleAcceptFriendAsync(long chatId, int messageId, int userId, string data,
        IFriendshipService friendshipService, CancellationToken ct)
    {
        var friendshipId = int.Parse(data.Replace(CallbackData.AcceptFriend, ""));
        var success = await friendshipService.AcceptFriendRequestAsync(friendshipId, userId);

        await _bot.SendMessage(chatId,
            success ? "✅ Вы приняли запрос в друзья!" : "❌ Не удалось принять запрос.",
            replyMarkup: _keyboard.GetFriendsMenuReplyKeyboard(),
            cancellationToken: ct);
    }

    private async Task HandleRejectFriendAsync(long chatId, int messageId, int userId, string data,
        IFriendshipService friendshipService, CancellationToken ct)
    {
        var friendshipId = int.Parse(data.Replace(CallbackData.RejectFriend, ""));
        await friendshipService.RejectFriendRequestAsync(friendshipId, userId);

        await _bot.SendMessage(chatId,
            "❌ Запрос отклонён.",
            replyMarkup: _keyboard.GetFriendsMenuReplyKeyboard(),
            cancellationToken: ct);
    }

    private async Task HandleInviteFriendAsync(long chatId, int messageId, long telegramId, int userId,
        string data, IGameService gameService, IUserService userService, CancellationToken ct)
    {
        var friendId = int.Parse(data.Replace(CallbackData.InviteFriend, ""));
        var friend = await userService.GetByIdAsync(friendId);

        if (friend == null)
        {
            await _bot.SendMessage(chatId,
                "❌ Пользователь не найден.",
                replyMarkup: _keyboard.GetMainMenuReplyKeyboard(),
                cancellationToken: ct);
            return;
        }

        var session = await gameService.CreateFriendGameAsync(telegramId, friend.TelegramId);

        await _bot.SendMessage(chatId,
            "📨 Приглашение отправлено! Ожидаем ответа...",
            replyMarkup: _keyboard.GetReadyKeyboard(),
            cancellationToken: ct);

        var inviter = await userService.GetByTelegramIdAsync(telegramId);
        var inviterName = inviter?.Username ?? inviter?.FirstName ?? "Друг";

        await _bot.SendMessage(friend.TelegramId,
            $"🎮 *{inviterName}* приглашает вас в игру!",
            parseMode: ParseMode.Markdown,
            replyMarkup: _keyboard.GetReadyKeyboard(),
            cancellationToken: ct);
    }
}
