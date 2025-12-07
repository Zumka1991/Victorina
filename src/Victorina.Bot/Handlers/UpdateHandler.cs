using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Victorina.Application.Interfaces;
using Victorina.Bot.Constants;
using Victorina.Bot.Services;
using Victorina.Domain.Entities;
using Victorina.Domain.Enums;

namespace Victorina.Bot.Handlers;

public class UpdateHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly IServiceProvider _serviceProvider;
    private readonly KeyboardService _keyboard;
    private readonly UserStateService _userState;
    private readonly ILogger<UpdateHandler> _logger;
    private readonly string _apiBaseUrl;

    public UpdateHandler(
        ITelegramBotClient bot,
        IServiceProvider serviceProvider,
        KeyboardService keyboard,
        UserStateService userState,
        ILogger<UpdateHandler> logger,
        IConfiguration configuration)
    {
        _bot = bot;
        _serviceProvider = serviceProvider;
        _keyboard = keyboard;
        _userState = userState;
        _logger = logger;
        _apiBaseUrl = configuration["Api:BaseUrl"] ?? "http://localhost:5175";
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
        var questionService = scope.ServiceProvider.GetRequiredService<IQuestionService>();

        var telegramUser = message.From!;
        var user = await userService.GetOrCreateUserAsync(
            telegramUser.Id,
            telegramUser.Username,
            telegramUser.FirstName,
            telegramUser.LastName);

        var chatId = message.Chat.Id;
        var state = _userState.GetState(telegramUser.Id);
        var lang = user.LanguageCode;

        // Обработка состояния поиска друга
        if (state == UserState.WaitingForFriendSearch)
        {
            // Check for back/cancel buttons in any language
            if (IsBackButton(text) || IsCancelButton(text) || IsBackToProfileButton(text))
            {
                _userState.ClearState(telegramUser.Id);
                await SendFriendsMenu(chatId, lang, ct);
                return;
            }
            await HandleFriendSearchAsync(chatId, telegramUser.Id, user.Id, lang, text, ct);
            return;
        }

        // Обработка Reply-кнопок - проверяем на всех языках
        if (text == "/start" || text == "/menu" || IsBackButton(text))
        {
            await SendMainMenu(chatId, lang, ct);
        }
        else if (IsBackToProfileButton(text))
        {
            await SendProfileMenu(chatId, lang, ct);
        }
        else if (IsPlayButton(text))
        {
            await SendPlayMenu(chatId, lang, ct);
        }
        else if (IsQuickGameButton(text))
        {
            await SendCategoryGroupSelectionAsync(chatId, lang, false, null, ct);
        }
        else if (IsPlayWithFriendButton(text))
        {
            await HandlePlayWithFriendReplyAsync(chatId, user.Id, lang, friendshipService, ct);
        }
        else if (IsProfileButton(text))
        {
            await SendProfileMenu(chatId, lang, ct);
        }
        else if (IsStatisticsButton(text))
        {
            await HandleStatisticsReplyAsync(chatId, telegramUser.Id, lang, gameService, ct);
        }
        else if (IsLanguageButton(text))
        {
            await HandleLanguageSelectionAsync(chatId, user, ct);
        }
        else if (IsLeadersButton(text))
        {
            await HandleLeaderboardReplyAsync(chatId, telegramUser.Id, lang, gameService, ct);
        }
        else if (IsFriendsButton(text))
        {
            await SendFriendsMenu(chatId, lang, ct);
        }
        else if (IsMyFriendsButton(text))
        {
            await HandleFriendsListReplyAsync(chatId, user.Id, lang, friendshipService, ct);
        }
        else if (IsAddFriendButton(text))
        {
            _userState.SetState(telegramUser.Id, UserState.WaitingForFriendSearch);
            await _bot.SendMessage(chatId,
                LocalizationService.Get(lang, "friend_search"),
                replyMarkup: _keyboard.GetCancelReplyKeyboard(lang),
                cancellationToken: ct);
        }
        else if (IsCancelButton(text) || IsLeaveGameButton(text))
        {
            var activeGame = await gameService.GetActiveGameAsync(telegramUser.Id);
            if (activeGame != null)
            {
                await gameService.CancelGameAsync(activeGame.GameId);
                foreach (var player in activeGame.Players.Values.Where(p => p.TelegramId != telegramUser.Id))
                {
                    var opponentLang = await GetUserLanguageAsync(player.TelegramId, userService);
                    await _bot.SendMessage(player.TelegramId,
                        LocalizationService.Get(opponentLang, "opponent_left"),
                        replyMarkup: _keyboard.GetMainMenuReplyKeyboard(opponentLang),
                        cancellationToken: ct);
                }
            }
            await SendMainMenu(chatId, lang, ct);
        }
        else if (text == "/help" || IsHelpButton(text))
        {
            await _bot.SendMessage(chatId,
                LocalizationService.Get(lang, "help"),
                parseMode: ParseMode.Markdown,
                replyMarkup: _keyboard.GetMainMenuReplyKeyboard(lang),
                cancellationToken: ct);
        }
        else if (text.StartsWith("/"))
        {
            await SendMainMenu(chatId, lang, ct);
        }
    }

    // Button detection helpers - check all languages
    private static bool IsBackButton(string text) =>
        LocalizationService.Languages.Keys.Any(l => text == LocalizationService.Get(l, "btn_back"));

    private static bool IsBackToProfileButton(string text) =>
        LocalizationService.Languages.Keys.Any(l => text == LocalizationService.Get(l, "btn_back_to_profile"));

    private static bool IsPlayButton(string text) =>
        LocalizationService.Languages.Keys.Any(l => text == LocalizationService.Get(l, "btn_play"));

    private static bool IsQuickGameButton(string text) =>
        LocalizationService.Languages.Keys.Any(l => text == LocalizationService.Get(l, "btn_quick_game"));

    private static bool IsPlayWithFriendButton(string text) =>
        LocalizationService.Languages.Keys.Any(l => text == LocalizationService.Get(l, "btn_play_with_friend"));

    private static bool IsProfileButton(string text) =>
        LocalizationService.Languages.Keys.Any(l => text == LocalizationService.Get(l, "btn_profile"));

    private static bool IsStatisticsButton(string text) =>
        LocalizationService.Languages.Keys.Any(l => text == LocalizationService.Get(l, "btn_statistics"));

    private static bool IsLanguageButton(string text) =>
        LocalizationService.Languages.Keys.Any(l => text == LocalizationService.Get(l, "btn_language"));

    private static bool IsLeadersButton(string text) =>
        LocalizationService.Languages.Keys.Any(l => text == LocalizationService.Get(l, "btn_leaders"));

    private static bool IsFriendsButton(string text) =>
        LocalizationService.Languages.Keys.Any(l => text == LocalizationService.Get(l, "btn_friends"));

    private static bool IsMyFriendsButton(string text) =>
        LocalizationService.Languages.Keys.Any(l => text == LocalizationService.Get(l, "btn_my_friends"));

    private static bool IsAddFriendButton(string text) =>
        LocalizationService.Languages.Keys.Any(l => text == LocalizationService.Get(l, "btn_add_friend"));

    private static bool IsRequestsButton(string text) =>
        LocalizationService.Languages.Keys.Any(l => text == LocalizationService.Get(l, "btn_requests"));

    private static bool IsCancelButton(string text) =>
        LocalizationService.Languages.Keys.Any(l => text == LocalizationService.Get(l, "btn_cancel"));

    private static bool IsLeaveGameButton(string text) =>
        LocalizationService.Languages.Keys.Any(l => text == LocalizationService.Get(l, "btn_leave_game"));

    private static bool IsHelpButton(string text) =>
        LocalizationService.Languages.Keys.Any(l => text == LocalizationService.Get(l, "btn_help"));

    private async Task<string> GetUserLanguageAsync(long telegramId, IUserService userService)
    {
        var user = await userService.GetByTelegramIdAsync(telegramId);
        return user?.LanguageCode ?? "ru";
    }

    private async Task SendMainMenu(long chatId, string lang, CancellationToken ct)
    {
        await _bot.SendMessage(chatId,
            LocalizationService.Get(lang, "welcome"),
            parseMode: ParseMode.Markdown,
            replyMarkup: _keyboard.GetMainMenuReplyKeyboard(lang),
            cancellationToken: ct);
    }

    private async Task SendPlayMenu(long chatId, string lang, CancellationToken ct)
    {
        await _bot.SendMessage(chatId,
            LocalizationService.Get(lang, "play_menu"),
            parseMode: ParseMode.Markdown,
            replyMarkup: _keyboard.GetPlayMenuReplyKeyboard(lang),
            cancellationToken: ct);
    }

    private async Task SendProfileMenu(long chatId, string lang, CancellationToken ct)
    {
        await _bot.SendMessage(chatId,
            LocalizationService.Get(lang, "profile_menu"),
            parseMode: ParseMode.Markdown,
            replyMarkup: _keyboard.GetProfileMenuReplyKeyboard(lang),
            cancellationToken: ct);
    }

    private async Task SendFriendsMenu(long chatId, string lang, CancellationToken ct)
    {
        await _bot.SendMessage(chatId,
            LocalizationService.Get(lang, "friends_menu"),
            parseMode: ParseMode.Markdown,
            replyMarkup: _keyboard.GetFriendsMenuReplyKeyboard(lang),
            cancellationToken: ct);
    }

    private async Task SendCategoryGroupSelectionAsync(long chatId, string lang, bool forFriend, int? friendId, CancellationToken ct)
    {
        await _bot.SendMessage(chatId,
            LocalizationService.Get(lang, "category_groups"),
            parseMode: ParseMode.Markdown,
            replyMarkup: _keyboard.GetCategoryGroupSelectionKeyboard(lang, forFriend, friendId),
            cancellationToken: ct);
    }

    private async Task SendCategorySelectionAsync(long chatId, string lang, IQuestionService questionService,
        bool forFriend, int? friendId, CancellationToken ct)
    {
        var categories = await questionService.GetCategoriesAsync(lang);
        var message = forFriend
            ? LocalizationService.Get(lang, "select_category_friend")
            : LocalizationService.Get(lang, "select_category");

        await _bot.SendMessage(chatId,
            message,
            parseMode: ParseMode.Markdown,
            replyMarkup: _keyboard.GetCategorySelectionKeyboard(categories, lang, forFriend, friendId),
            cancellationToken: ct);
    }

    private async Task HandleQuickGameReplyAsync(long chatId, long telegramId, string lang,
        int? categoryId, IGameService gameService, IUserService userService, IQuestionService questionService,
        CancellationToken ct)
    {
        var activeGame = await gameService.GetActiveGameAsync(telegramId);
        if (activeGame != null)
        {
            await _bot.SendMessage(chatId,
                LocalizationService.Get(lang, "active_game_exists"),
                replyMarkup: _keyboard.GetGameReplyKeyboard(lang),
                cancellationToken: ct);
            return;
        }

        var session = await gameService.FindQuickGameAsync(telegramId, categoryId);

        if (session != null)
        {
            session = await gameService.JoinGameAsync(session.GameId, telegramId);
            var opponent = session!.Players.Values.First(p => p.TelegramId != telegramId);
            var currentPlayer = session.Players.Values.First(p => p.TelegramId == telegramId);

            var opponentFlag = CountryService.GetFlag(opponent.CountryCode);
            var opponentName = opponent.GetDisplayName();
            var currentPlayerFlag = CountryService.GetFlag(currentPlayer.CountryCode);
            var currentPlayerName = currentPlayer.GetDisplayName();

            // Get category name in current player's language
            var categoryName = await questionService.GetCategoryNameAsync(
                session.CategoryTranslationGroupId, session.CategoryId, lang);
            var categoryInfo = categoryName != null
                ? LocalizationService.Get(lang, "category_info", categoryName)
                : "";

            await _bot.SendMessage(chatId,
                $"{LocalizationService.Get(lang, "opponent_found")}\n\n{opponentFlag} *{opponentName}*{categoryInfo}\n\n{LocalizationService.Get(lang, "btn_ready")}!",
                parseMode: ParseMode.Markdown,
                replyMarkup: _keyboard.GetReadyKeyboard(lang),
                cancellationToken: ct);

            var opponentLang = await GetUserLanguageAsync(opponent.TelegramId, userService);
            // Get category name in opponent's language
            var opponentCategoryName = await questionService.GetCategoryNameAsync(
                session.CategoryTranslationGroupId, session.CategoryId, opponentLang);
            var opponentCategoryInfo = opponentCategoryName != null
                ? LocalizationService.Get(opponentLang, "category_info", opponentCategoryName)
                : "";

            await _bot.SendMessage(opponent.TelegramId,
                $"{LocalizationService.Get(opponentLang, "opponent_found")}\n\n{currentPlayerFlag} *{currentPlayerName}*{opponentCategoryInfo}\n\n{LocalizationService.Get(opponentLang, "btn_ready")}!",
                parseMode: ParseMode.Markdown,
                replyMarkup: _keyboard.GetReadyKeyboard(opponentLang),
                cancellationToken: ct);
        }
        else
        {
            await gameService.CreateQuickGameAsync(telegramId, categoryId);

            var waitingText = categoryId.HasValue
                ? LocalizationService.Get(lang, "searching_category")
                : LocalizationService.Get(lang, "searching_opponent");

            await _bot.SendMessage(chatId,
                waitingText,
                parseMode: ParseMode.Markdown,
                replyMarkup: _keyboard.GetSearchingKeyboard(lang),
                cancellationToken: ct);
        }
    }

    private async Task HandleStatisticsReplyAsync(long chatId, long telegramId, string lang,
        IGameService gameService, CancellationToken ct)
    {
        var stats = await gameService.GetUserStatsAsync(telegramId);

        await _bot.SendMessage(chatId,
            LocalizationService.Get(lang, "your_statistics",
                stats.GamesPlayed, stats.GamesWon, stats.WinRate.ToString("F1"), stats.TotalCorrectAnswers),
            parseMode: ParseMode.Markdown,
            replyMarkup: _keyboard.GetProfileMenuReplyKeyboard(lang),
            cancellationToken: ct);
    }

    private async Task HandleLeaderboardReplyAsync(long chatId, long telegramId, string lang,
        IGameService gameService, CancellationToken ct)
    {
        var leaderboard = await gameService.GetLeaderboardAsync(10);
        var userRank = await gameService.GetUserRankAsync(telegramId);

        if (leaderboard.Count == 0)
        {
            await _bot.SendMessage(chatId,
                LocalizationService.Get(lang, "leaderboard_empty"),
                parseMode: ParseMode.Markdown,
                replyMarkup: _keyboard.GetMainMenuReplyKeyboard(lang),
                cancellationToken: ct);
            return;
        }

        var medals = new[] { "🥇", "🥈", "🥉" };
        var message = LocalizationService.Get(lang, "leaderboard") + "\n\n";

        foreach (var entry in leaderboard)
        {
            var medal = entry.Rank <= 3 ? medals[entry.Rank - 1] : $"{entry.Rank}.";
            var name = !string.IsNullOrEmpty(entry.Username) ? $"@{entry.Username}" : entry.FirstName ?? LocalizationService.Get(lang, "player");
            message += $"{medal} *{name}*\n" +
                      $"    🏆 {entry.GamesWon} {LocalizationService.Get(lang, "wins")} • 🎮 {entry.GamesPlayed} {LocalizationService.Get(lang, "games")} • {entry.WinRate:F0}%\n\n";
        }

        if (userRank.HasValue)
        {
            message += $"━━━━━━━━━━━━━━━\n" +
                      LocalizationService.Get(lang, "your_position", userRank.Value.Rank) + $"\n" +
                      $"🏆 {userRank.Value.Stats.GamesWon} {LocalizationService.Get(lang, "wins")} / {userRank.Value.Stats.GamesPlayed} {LocalizationService.Get(lang, "games")}";
        }
        else
        {
            message += $"━━━━━━━━━━━━━━━\n" +
                      LocalizationService.Get(lang, "play_to_rank");
        }

        await _bot.SendMessage(chatId,
            message,
            parseMode: ParseMode.Markdown,
            replyMarkup: _keyboard.GetMainMenuReplyKeyboard(lang),
            cancellationToken: ct);
    }

    private async Task HandleLanguageSelectionAsync(long chatId, Domain.Entities.User user, CancellationToken ct)
    {
        var (flag, name) = LocalizationService.GetLanguageInfo(user.LanguageCode);

        await _bot.SendMessage(chatId,
            LocalizationService.Get(user.LanguageCode, "language_selection", flag, name),
            parseMode: ParseMode.Markdown,
            replyMarkup: _keyboard.GetLanguageSelectionKeyboard(user.LanguageCode),
            cancellationToken: ct);
    }

    private async Task HandleFriendsListReplyAsync(long chatId, int userId, string lang,
        IFriendshipService friendshipService, CancellationToken ct)
    {
        var friends = await friendshipService.GetFriendsAsync(userId);

        if (friends.Count == 0)
        {
            await _bot.SendMessage(chatId,
                LocalizationService.Get(lang, "no_friends"),
                replyMarkup: _keyboard.GetFriendsMenuReplyKeyboard(lang),
                cancellationToken: ct);
        }
        else
        {
            await _bot.SendMessage(chatId,
                LocalizationService.Get(lang, "select_friend"),
                parseMode: ParseMode.Markdown,
                replyMarkup: _keyboard.GetFriendsListKeyboard(friends, lang),
                cancellationToken: ct);
        }
    }

    private async Task HandleFriendRequestsReplyAsync(long chatId, int userId, string lang,
        IFriendshipService friendshipService, CancellationToken ct)
    {
        var requests = await friendshipService.GetPendingRequestsAsync(userId);

        if (requests.Count == 0)
        {
            await _bot.SendMessage(chatId,
                LocalizationService.Get(lang, "no_requests"),
                replyMarkup: _keyboard.GetFriendsMenuReplyKeyboard(lang),
                cancellationToken: ct);
        }
        else
        {
            await _bot.SendMessage(chatId,
                LocalizationService.Get(lang, "incoming_requests"),
                parseMode: ParseMode.Markdown,
                replyMarkup: _keyboard.GetFriendRequestsKeyboard(requests, lang),
                cancellationToken: ct);
        }
    }

    private async Task HandlePlayWithFriendReplyAsync(long chatId, int userId, string lang,
        IFriendshipService friendshipService, CancellationToken ct)
    {
        var friends = await friendshipService.GetFriendsAsync(userId);

        if (friends.Count == 0)
        {
            await _bot.SendMessage(chatId,
                LocalizationService.Get(lang, "no_friends_for_game"),
                replyMarkup: _keyboard.GetPlayMenuReplyKeyboard(lang),
                cancellationToken: ct);
        }
        else
        {
            await _bot.SendMessage(chatId,
                LocalizationService.Get(lang, "select_friend"),
                parseMode: ParseMode.Markdown,
                replyMarkup: _keyboard.GetFriendsListKeyboard(friends, lang),
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
        var questionService = scope.ServiceProvider.GetRequiredService<IQuestionService>();

        var user = await userService.GetOrCreateUserAsync(
            callback.From.Id,
            callback.From.Username,
            callback.From.FirstName,
            callback.From.LastName);

        var lang = user.LanguageCode;

        try
        {
            switch (data)
            {
                case CallbackData.QuickGame:
                    await SendCategoryGroupSelectionAsync(chatId, lang, false, null, ct);
                    break;

                case CallbackData.CheckGame:
                    await HandleCheckGameAsync(chatId, messageId, telegramId, lang, gameService, userService, ct);
                    break;

                case CallbackData.Ready:
                    await HandleReadyAsync(chatId, messageId, telegramId, lang, gameService, userService, ct);
                    break;

                case CallbackData.CheckOpponent:
                    await HandleCheckOpponentAsync(chatId, messageId, telegramId, lang, gameService, ct);
                    break;

                case CallbackData.CancelGame:
                    await HandleCancelGameAsync(chatId, messageId, telegramId, lang, gameService, userService, ct);
                    break;

                case CallbackData.PlayWithFriend:
                    await HandlePlayWithFriendReplyAsync(chatId, user.Id, lang, friendshipService, ct);
                    break;

                case CallbackData.Friends:
                    await SendFriendsMenu(chatId, lang, ct);
                    break;

                case CallbackData.AddFriend:
                    _userState.SetState(telegramId, UserState.WaitingForFriendSearch);
                    await _bot.SendMessage(chatId,
                        LocalizationService.Get(lang, "friend_search"),
                        replyMarkup: _keyboard.GetCancelReplyKeyboard(lang),
                        cancellationToken: ct);
                    break;

                case CallbackData.BackToMenu:
                    _userState.ClearState(telegramId);
                    await SendMainMenu(chatId, lang, ct);
                    break;

                case CallbackData.BackToProfile:
                    await SendProfileMenu(chatId, lang, ct);
                    break;

                default:
                    if (data.StartsWith(CallbackData.SelectCategoryGroup))
                    {
                        await HandleSelectCategoryGroupAsync(chatId, messageId, telegramId, lang, data, questionService, userService, ct);
                    }
                    else if (data.StartsWith(CallbackData.SelectCategoryGroupForFriend))
                    {
                        await HandleSelectCategoryGroupForFriendAsync(chatId, messageId, telegramId, lang, data, questionService, userService, ct);
                    }
                    else if (data.StartsWith(CallbackData.SelectCategory))
                    {
                        await HandleSelectCategoryAsync(chatId, messageId, telegramId, lang, data, gameService, userService, questionService, ct);
                    }
                    else if (data.StartsWith(CallbackData.SelectCategoryForFriend))
                    {
                        await HandleSelectCategoryForFriendAsync(chatId, messageId, telegramId, lang, data,
                            gameService, userService, questionService, ct);
                    }
                    else if (data.StartsWith(CallbackData.SelectLanguage))
                    {
                        await HandleSelectLanguageAsync(chatId, messageId, user.Id, data, userService, ct);
                    }
                    else if (data.StartsWith(CallbackData.Answer))
                    {
                        await HandleAnswerAsync(chatId, messageId, telegramId, lang, data, gameService, userService, ct);
                    }
                    else if (data.StartsWith(CallbackData.InviteFriend))
                    {
                        await HandleInviteFriendAsync(chatId, messageId, telegramId, user.Id, lang, data,
                            gameService, userService, questionService, ct);
                    }
                    else if (data.StartsWith(CallbackData.AcceptGameInvite))
                    {
                        await HandleAcceptGameInviteAsync(chatId, messageId, telegramId, user.Id, lang, data, ct);
                    }
                    else if (data.StartsWith(CallbackData.DeclineGameInvite))
                    {
                        await HandleDeclineGameInviteAsync(chatId, messageId, telegramId, user.Id, lang, data, userService, ct);
                    }
                    break;
            }

            await _bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling callback {Data}", data);
            await _bot.AnswerCallbackQuery(callback.Id, "Error", cancellationToken: ct);
        }
    }

    private async Task HandleCheckGameAsync(long chatId, int messageId, long telegramId, string lang,
        IGameService gameService, IUserService userService, CancellationToken ct)
    {
        var session = await gameService.GetActiveGameAsync(telegramId);

        if (session == null)
        {
            await _bot.SendMessage(chatId,
                LocalizationService.Get(lang, "game_not_found"),
                replyMarkup: _keyboard.GetMainMenuReplyKeyboard(lang),
                cancellationToken: ct);
            return;
        }

        if (session.Status == GameStatus.WaitingForPlayers)
        {
            await _bot.SendMessage(chatId,
                LocalizationService.Get(lang, "searching_opponent"),
                parseMode: ParseMode.Markdown,
                cancellationToken: ct);
        }
        else if (session.Status == GameStatus.WaitingForReady)
        {
            var opponent = session.Players.Values.First(p => p.TelegramId != telegramId);
            var opponentFlag = CountryService.GetFlag(opponent.CountryCode);
            var opponentName = opponent.GetDisplayName();
            await _bot.SendMessage(chatId,
                $"{LocalizationService.Get(lang, "opponent_found")}\n\n{opponentFlag} *{opponentName}*\n\n{LocalizationService.Get(lang, "btn_ready")}!",
                parseMode: ParseMode.Markdown,
                replyMarkup: _keyboard.GetReadyKeyboard(lang),
                cancellationToken: ct);
        }
    }

    private async Task HandleReadyAsync(long chatId, int messageId, long telegramId, string lang,
        IGameService gameService, IUserService userService, CancellationToken ct)
    {
        var session = await gameService.GetActiveGameAsync(telegramId);
        if (session == null) return;

        await gameService.SetPlayerReadyAsync(session.GameId, telegramId);
        session = await gameService.GetActiveGameAsync(telegramId);
        if (session == null) return;

        if (session.Status == GameStatus.InProgress)
        {
            foreach (var player in session.Players.Values)
            {
                // Get first question in player's language
                var playerQuestion = player.Questions.Count > 0
                    ? player.Questions[0]
                    : session.Questions[0];

                var playerLang = await GetUserLanguageAsync(player.TelegramId, userService);
                await _bot.SendMessage(player.TelegramId,
                    LocalizationService.Get(playerLang, "game_starting"),
                    parseMode: ParseMode.Markdown,
                    replyMarkup: _keyboard.GetGameReplyKeyboard(playerLang),
                    cancellationToken: ct);

                await Task.Delay(1000, ct);

                await SendQuestionAsync(player.TelegramId, playerLang, playerQuestion, 1, session.Questions.Count, ct);
            }
        }
        else
        {
            await _bot.SendMessage(chatId,
                LocalizationService.Get(lang, "waiting_ready"),
                replyMarkup: _keyboard.GetWaitingOpponentKeyboard(lang),
                cancellationToken: ct);
        }
    }

    private async Task HandleAnswerAsync(long chatId, int messageId, long telegramId, string lang, string data,
        IGameService gameService, IUserService userService, CancellationToken ct)
    {
        var answerIndex = int.Parse(data.Replace(CallbackData.Answer, ""));

        var session = await gameService.GetActiveGameAsync(telegramId);
        if (session == null || session.Status != GameStatus.InProgress) return;

        // Получаем текущий вопрос до ответа, чтобы знать есть ли картинка
        var currentPlayer = session.Players[telegramId];
        var currentQuestion = currentPlayer.Questions.Count > session.CurrentQuestionIndex
            ? currentPlayer.Questions[session.CurrentQuestionIndex]
            : session.Questions[session.CurrentQuestionIndex];
        var hasImage = !string.IsNullOrEmpty(currentQuestion.ImageUrl);

        var result = await gameService.SubmitAnswerAsync(session.GameId, telegramId, answerIndex);

        var emoji = result.IsCorrect ? "✅" : "❌";
        var userAnswer = currentQuestion.Answers[answerIndex];
        var resultText = $"{emoji} {(result.IsCorrect ? LocalizationService.Get(lang, "correct") : LocalizationService.Get(lang, "incorrect"))}\n\n" +
            $"*{LocalizationService.Get(lang, "question_label")}:* {currentQuestion.Text}\n\n" +
            LocalizationService.Get(lang, "your_answer", userAnswer) + "\n" +
            LocalizationService.Get(lang, "correct_answer", result.CorrectAnswer) + "\n" +
            LocalizationService.Get(lang, "your_time", (result.TimeMs / 1000.0).ToString("F2"));

        if (hasImage)
        {
            // Для вопроса с картинкой - удаляем сообщение и отправляем результат текстом
            try { await _bot.DeleteMessage(chatId, messageId, ct); } catch { }
            await _bot.SendMessage(chatId, resultText, parseMode: ParseMode.Markdown, cancellationToken: ct);
        }
        else
        {
            // Для текстовых сообщений редактируем текст
            await _bot.EditMessageText(chatId, messageId,
                resultText,
                parseMode: ParseMode.Markdown,
                cancellationToken: ct);
        }

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
                    await SendGameResultsAsync(gameResult, userService, ct);
                }
            }
            else
            {
                await gameService.MoveToNextQuestionAsync(session.GameId);
                session = await gameService.GetGameByIdAsync(session.GameId);

                if (session != null)
                {
                    foreach (var p in session.Players.Values)
                    {
                        // Get question in player's language
                        var playerQuestion = p.Questions.Count > session.CurrentQuestionIndex
                            ? p.Questions[session.CurrentQuestionIndex]
                            : session.Questions[session.CurrentQuestionIndex];

                        var pLang = await GetUserLanguageAsync(p.TelegramId, userService);
                        await Task.Delay(1500, ct);
                        await SendQuestionAsync(p.TelegramId, pLang, playerQuestion,
                            session.CurrentQuestionIndex + 1, session.Questions.Count, ct);
                    }
                }
            }
        }
        else
        {
            await Task.Delay(500, ct);
            await _bot.SendMessage(chatId,
                LocalizationService.Get(lang, "opponent_answering"),
                replyMarkup: _keyboard.GetWaitingOpponentKeyboard(lang),
                cancellationToken: ct);
        }
    }

    private async Task HandleCheckOpponentAsync(long chatId, int messageId, long telegramId, string lang,
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
                // Get question in player's language
                var question = player.Questions.Count > session.CurrentQuestionIndex
                    ? player.Questions[session.CurrentQuestionIndex]
                    : session.Questions[session.CurrentQuestionIndex];
                await SendQuestionAsync(chatId, lang, question,
                    session.CurrentQuestionIndex + 1, session.Questions.Count, ct);
            }
        }
        else
        {
            await _bot.SendMessage(chatId,
                LocalizationService.Get(lang, "opponent_still_answering"),
                cancellationToken: ct);
        }
    }

    private static readonly HttpClient _httpClient = new();

    private async Task SendQuestionAsync(long chatId, string lang, Application.Models.GameSessionQuestion question,
        int questionNumber, int totalQuestions, CancellationToken ct)
    {
        var questionText = LocalizationService.Get(lang, "question", questionNumber, totalQuestions) + $"\n\n{question.Text}";

        if (!string.IsNullOrEmpty(question.ImageUrl))
        {
            try
            {
                // Формируем полный URL для картинки
                var imageUrl = question.ImageUrl.StartsWith("http")
                    ? question.ImageUrl
                    : $"{_apiBaseUrl}{question.ImageUrl}";

                // Скачиваем картинку и отправляем как stream
                var imageBytes = await _httpClient.GetByteArrayAsync(imageUrl, ct);
                using var stream = new MemoryStream(imageBytes);
                var fileName = Path.GetFileName(question.ImageUrl);

                await _bot.SendPhoto(chatId,
                    new Telegram.Bot.Types.InputFileStream(stream, fileName),
                    caption: questionText,
                    parseMode: ParseMode.Markdown,
                    replyMarkup: _keyboard.GetQuestionKeyboard(question.Answers),
                    cancellationToken: ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send image, sending text only");
                // Если не удалось отправить картинку - отправляем только текст
                await _bot.SendMessage(chatId,
                    questionText,
                    parseMode: ParseMode.Markdown,
                    replyMarkup: _keyboard.GetQuestionKeyboard(question.Answers),
                    cancellationToken: ct);
            }
        }
        else
        {
            // Отправляем просто текст с кнопками
            await _bot.SendMessage(chatId,
                questionText,
                parseMode: ParseMode.Markdown,
                replyMarkup: _keyboard.GetQuestionKeyboard(question.Answers),
                cancellationToken: ct);
        }
    }

    private async Task SendGameResultsAsync(Application.Models.GameResult result, IUserService userService, CancellationToken ct)
    {
        foreach (var playerResult in new[] { result.Player1, result.Player2 })
        {
            var lang = await GetUserLanguageAsync(playerResult.TelegramId, userService);
            var isWinner = result.WinnerTelegramId == playerResult.TelegramId;
            var opponent = playerResult == result.Player1 ? result.Player2 : result.Player1;

            string title;
            if (result.IsDraw)
            {
                title = LocalizationService.Get(lang, "draw");
            }
            else if (isWinner)
            {
                title = LocalizationService.Get(lang, "you_won");
            }
            else
            {
                title = LocalizationService.Get(lang, "you_lost");
            }

            var opponentFlag = CountryService.GetFlag(opponent.CountryCode);
            var opponentName = opponent.GetDisplayName();

            var message = $"{title}\n\n" +
                         LocalizationService.Get(lang, "your_result") + $"\n" +
                         LocalizationService.Get(lang, "correct_answers", playerResult.CorrectAnswers) + $"\n" +
                         LocalizationService.Get(lang, "time_spent", playerResult.TotalTime.TotalSeconds.ToString("F2")) + $"\n\n" +
                         LocalizationService.Get(lang, "opponent_result", opponentFlag, opponentName) + $"\n" +
                         LocalizationService.Get(lang, "correct_answers", opponent.CorrectAnswers) + $"\n" +
                         LocalizationService.Get(lang, "time_spent", opponent.TotalTime.TotalSeconds.ToString("F2"));

            if (!result.IsDraw)
            {
                var winReason = result.WinReason?.Contains("ответ") == true || result.WinReason?.Contains("answer") == true
                    ? LocalizationService.Get(lang, "win_by_answers")
                    : LocalizationService.Get(lang, "win_by_time");
                message += $"\n\n_{winReason}_";
            }

            await _bot.SendMessage(playerResult.TelegramId,
                message,
                parseMode: ParseMode.Markdown,
                replyMarkup: _keyboard.GetMainMenuReplyKeyboard(lang),
                cancellationToken: ct);
        }
    }

    private async Task HandleCancelGameAsync(long chatId, int messageId, long telegramId, string lang,
        IGameService gameService, IUserService userService, CancellationToken ct)
    {
        var session = await gameService.GetActiveGameAsync(telegramId);
        if (session != null)
        {
            await gameService.CancelGameAsync(session.GameId);

            foreach (var player in session.Players.Values.Where(p => p.TelegramId != telegramId))
            {
                var playerLang = await GetUserLanguageAsync(player.TelegramId, userService);
                await _bot.SendMessage(player.TelegramId,
                    LocalizationService.Get(playerLang, "opponent_cancelled"),
                    replyMarkup: _keyboard.GetMainMenuReplyKeyboard(playerLang),
                    cancellationToken: ct);
            }
        }

        await _bot.SendMessage(chatId,
            LocalizationService.Get(lang, "game_cancelled"),
            replyMarkup: _keyboard.GetMainMenuReplyKeyboard(lang),
            cancellationToken: ct);
    }

    private async Task HandleFriendSearchAsync(long chatId, long telegramId, int userId, string lang,
        string searchText, CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var gameService = scope.ServiceProvider.GetRequiredService<IGameService>();
        var friendshipService = scope.ServiceProvider.GetRequiredService<IFriendshipService>();

        _userState.ClearState(telegramId);

        var searchQuery = searchText.TrimStart('@');
        var foundUser = await userService.FindByUsernameAsync(searchQuery)
                        ?? await userService.FindByPhoneAsync(searchQuery);

        if (foundUser == null || foundUser.Id == userId)
        {
            await _bot.SendMessage(chatId,
                LocalizationService.Get(lang, "friend_not_found"),
                replyMarkup: _keyboard.GetMainMenuReplyKeyboard(lang),
                cancellationToken: ct);
            return;
        }

        // Check if user or opponent has active game
        var userGame = await gameService.GetActiveGameAsync(telegramId);
        if (userGame != null)
        {
            await _bot.SendMessage(chatId,
                LocalizationService.Get(lang, "you_in_game"),
                replyMarkup: _keyboard.GetMainMenuReplyKeyboard(lang),
                cancellationToken: ct);
            return;
        }

        var opponentGame = await gameService.GetActiveGameAsync(foundUser.TelegramId);
        if (opponentGame != null)
        {
            await _bot.SendMessage(chatId,
                LocalizationService.Get(lang, "opponent_in_game"),
                replyMarkup: _keyboard.GetMainMenuReplyKeyboard(lang),
                cancellationToken: ct);
            return;
        }

        // Automatically add to friends if not already friends
        var areFriends = await friendshipService.AreFriendsAsync(userId, foundUser.Id);
        if (!areFriends)
        {
            // Create and auto-accept friend request
            var newRequest = await friendshipService.SendFriendRequestAsync(userId, foundUser.Id);
            if (newRequest != null)
            {
                // Auto-accept the friend request immediately
                await friendshipService.AcceptFriendRequestAsync(newRequest.Id, foundUser.Id);
            }
        }

        // Show category group selection to the inviter (works for both new and existing friends)
        await SendCategoryGroupSelectionAsync(chatId, lang, forFriend: true, foundUser.Id, ct);
    }

    private async Task HandleAcceptFriendAsync(long chatId, int messageId, int userId, string lang, string data,
        IFriendshipService friendshipService, CancellationToken ct)
    {
        var friendshipId = int.Parse(data.Replace(CallbackData.AcceptFriend, ""));
        var success = await friendshipService.AcceptFriendRequestAsync(friendshipId, userId);

        await _bot.SendMessage(chatId,
            success ? LocalizationService.Get(lang, "friend_accepted") : LocalizationService.Get(lang, "accept_failed"),
            replyMarkup: _keyboard.GetFriendsMenuReplyKeyboard(lang),
            cancellationToken: ct);
    }

    private async Task HandleRejectFriendAsync(long chatId, int messageId, int userId, string lang, string data,
        IFriendshipService friendshipService, CancellationToken ct)
    {
        var friendshipId = int.Parse(data.Replace(CallbackData.RejectFriend, ""));
        await friendshipService.RejectFriendRequestAsync(friendshipId, userId);

        await _bot.SendMessage(chatId,
            LocalizationService.Get(lang, "friend_rejected"),
            replyMarkup: _keyboard.GetFriendsMenuReplyKeyboard(lang),
            cancellationToken: ct);
    }

    private async Task HandleInviteFriendAsync(long chatId, int messageId, long telegramId, int userId, string lang,
        string data, IGameService gameService, IUserService userService, IQuestionService questionService,
        CancellationToken ct)
    {
        var friendId = int.Parse(data.Replace(CallbackData.InviteFriend, ""));
        var friend = await userService.GetByIdAsync(friendId);

        if (friend == null)
        {
            await _bot.SendMessage(chatId,
                LocalizationService.Get(lang, "friend_not_found"),
                replyMarkup: _keyboard.GetMainMenuReplyKeyboard(lang),
                cancellationToken: ct);
            return;
        }

        // Показываем выбор группы категорий для игры с другом
        await SendCategoryGroupSelectionAsync(chatId, lang, true, friendId, ct);
    }

    private async Task HandleSelectCategoryGroupAsync(long chatId, int messageId, long telegramId, string lang,
        string data, IQuestionService questionService, IUserService userService, CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var gameService = scope.ServiceProvider.GetRequiredService<IGameService>();

        // grp_general, grp_special, grp_popular, grp_my, grp_all
        var groupName = data.Replace(CallbackData.SelectCategoryGroup, "");

        // If "any category" is selected, immediately start quick game with null categoryId
        if (groupName == "all")
        {
            await _bot.EditMessageText(chatId, messageId,
                LocalizationService.Get(lang, "searching_opponent"),
                parseMode: ParseMode.Markdown,
                cancellationToken: ct);

            await HandleQuickGameReplyAsync(chatId, telegramId, lang, null, gameService, userService, questionService, ct);
            return;
        }

        IList<Category> categories;
        if (groupName == "my")
        {
            categories = await questionService.GetUserCategoriesAsync(telegramId, lang);
        }
        else
        {
            categories = await questionService.GetCategoriesByGroupAsync(groupName, lang);
        }

        if (categories.Count == 0)
        {
            await _bot.EditMessageText(chatId, messageId,
                LocalizationService.Get(lang, "no_categories_found"),
                parseMode: ParseMode.Markdown,
                replyMarkup: _keyboard.GetCategoryGroupSelectionKeyboard(lang, false, null),
                cancellationToken: ct);
        }
        else
        {
            await _bot.EditMessageText(chatId, messageId,
                LocalizationService.Get(lang, "select_category"),
                parseMode: ParseMode.Markdown,
                replyMarkup: _keyboard.GetCategorySelectionKeyboard(categories, lang, false, null),
                cancellationToken: ct);
        }
    }

    private async Task HandleSelectCategoryGroupForFriendAsync(long chatId, int messageId, long telegramId, string lang,
        string data, IQuestionService questionService, IUserService userService, CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var gameService = scope.ServiceProvider.GetRequiredService<IGameService>();

        // grpf_123_general, grpf_123_special, grpf_123_popular, grpf_123_my, grpf_123_all
        var parts = data.Replace(CallbackData.SelectCategoryGroupForFriend, "").Split('_');
        if (parts.Length < 2) return;

        var friendId = int.Parse(parts[0]);
        var groupName = parts[1];

        // If "any category" is selected, immediately create game with null categoryId
        if (groupName == "all")
        {
            var friend = await userService.GetByIdAsync(friendId);
            if (friend == null)
            {
                await _bot.SendMessage(chatId,
                    LocalizationService.Get(lang, "friend_not_found"),
                    replyMarkup: _keyboard.GetMainMenuReplyKeyboard(lang),
                    cancellationToken: ct);
                return;
            }

            var session = await gameService.CreateFriendGameAsync(telegramId, friend.TelegramId, null);

            await _bot.EditMessageText(chatId, messageId,
                $"{LocalizationService.Get(lang, "invite_sent")}\n\n{LocalizationService.Get(lang, "waiting_response")}",
                parseMode: ParseMode.Markdown,
                cancellationToken: ct);

            await _bot.SendMessage(chatId,
                LocalizationService.Get(lang, "click_ready"),
                replyMarkup: _keyboard.GetReadyKeyboard(lang),
                cancellationToken: ct);

            var inviter = await userService.GetByTelegramIdAsync(telegramId);
            var inviterFlag = CountryService.GetFlag(inviter?.CountryCode);
            var inviterName = inviter != null
                ? $"{inviterFlag} {inviter.FirstName ?? ""} {inviter.LastName ?? ""}".Trim()
                : LocalizationService.Get(lang, "player");

            if (!string.IsNullOrEmpty(inviter?.Username))
                inviterName += $" (@{inviter.Username})";

            var friendLang = friend.LanguageCode;
            await _bot.SendMessage(friend.TelegramId,
                LocalizationService.Get(friendLang, "game_invite", inviterName),
                parseMode: ParseMode.Markdown,
                replyMarkup: _keyboard.GetGameInviteKeyboard(friendLang, inviter.Id),
                cancellationToken: ct);
            return;
        }

        IList<Category> categories;
        if (groupName == "my")
        {
            categories = await questionService.GetUserCategoriesAsync(telegramId, lang);
        }
        else
        {
            categories = await questionService.GetCategoriesByGroupAsync(groupName, lang);
        }

        if (categories.Count == 0)
        {
            await _bot.EditMessageText(chatId, messageId,
                LocalizationService.Get(lang, "no_categories_found"),
                parseMode: ParseMode.Markdown,
                replyMarkup: _keyboard.GetCategoryGroupSelectionKeyboard(lang, true, friendId),
                cancellationToken: ct);
        }
        else
        {
            await _bot.EditMessageText(chatId, messageId,
                LocalizationService.Get(lang, "select_category_friend"),
                parseMode: ParseMode.Markdown,
                replyMarkup: _keyboard.GetCategorySelectionKeyboard(categories, lang, true, friendId),
                cancellationToken: ct);
        }
    }

    private async Task HandleSelectCategoryAsync(long chatId, int messageId, long telegramId, string lang,
        string data, IGameService gameService, IUserService userService, IQuestionService questionService,
        CancellationToken ct)
    {
        // cat_0 = любая категория, cat_1 = категория с id 1
        var categoryIdStr = data.Replace(CallbackData.SelectCategory, "");
        int? categoryId = int.TryParse(categoryIdStr, out var id) && id > 0 ? id : null;

        await _bot.EditMessageText(chatId, messageId,
            categoryId.HasValue ? LocalizationService.Get(lang, "searching_category") : LocalizationService.Get(lang, "searching_opponent"),
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);

        await HandleQuickGameReplyAsync(chatId, telegramId, lang, categoryId, gameService, userService, questionService, ct);
    }

    private async Task HandleSelectCategoryForFriendAsync(long chatId, int messageId, long telegramId, string lang,
        string data, IGameService gameService, IUserService userService, IQuestionService questionService,
        CancellationToken ct)
    {
        // catf_123_0 = friend id 123, любая категория
        // catf_123_1 = friend id 123, категория с id 1
        var parts = data.Replace(CallbackData.SelectCategoryForFriend, "").Split('_');
        if (parts.Length < 2) return;

        var friendId = int.Parse(parts[0]);
        int? categoryId = int.TryParse(parts[1], out var id) && id > 0 ? id : null;

        var friend = await userService.GetByIdAsync(friendId);
        if (friend == null)
        {
            await _bot.SendMessage(chatId,
                LocalizationService.Get(lang, "friend_not_found"),
                replyMarkup: _keyboard.GetMainMenuReplyKeyboard(lang),
                cancellationToken: ct);
            return;
        }

        var session = await gameService.CreateFriendGameAsync(telegramId, friend.TelegramId, categoryId);

        // Get category name in inviter's language
        var categoryName = await questionService.GetCategoryNameAsync(
            session.CategoryTranslationGroupId, session.CategoryId, lang);
        var categoryInfo = categoryName != null
            ? LocalizationService.Get(lang, "category_info", categoryName)
            : "";

        await _bot.EditMessageText(chatId, messageId,
            $"{LocalizationService.Get(lang, "invite_sent")}{categoryInfo}\n\n{LocalizationService.Get(lang, "waiting_response")}",
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);

        await _bot.SendMessage(chatId,
            LocalizationService.Get(lang, "click_ready"),
            replyMarkup: _keyboard.GetReadyKeyboard(lang),
            cancellationToken: ct);

        var inviter = await userService.GetByTelegramIdAsync(telegramId);
        var inviterFlag = CountryService.GetFlag(inviter?.CountryCode);
        var inviterName = inviter != null
            ? $"{inviterFlag} {inviter.FirstName ?? ""} {inviter.LastName ?? ""}".Trim()
            : LocalizationService.Get(lang, "player");

        if (!string.IsNullOrEmpty(inviter?.Username))
            inviterName += $" (@{inviter.Username})";

        var friendLang = friend.LanguageCode;
        // Get category name in friend's language
        var friendCategoryName = await questionService.GetCategoryNameAsync(
            session.CategoryTranslationGroupId, session.CategoryId, friendLang);
        var inviteCategoryInfo = friendCategoryName != null
            ? LocalizationService.Get(friendLang, "category_info", friendCategoryName)
            : "";

        await _bot.SendMessage(friend.TelegramId,
            LocalizationService.Get(friendLang, "game_invite", inviterName) + inviteCategoryInfo,
            parseMode: ParseMode.Markdown,
            replyMarkup: _keyboard.GetGameInviteKeyboard(friendLang, inviter.Id),
            cancellationToken: ct);
    }

    private async Task HandleAcceptGameInviteAsync(long chatId, int messageId, long telegramId, int userId,
        string lang, string data, CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var inviterId = int.Parse(data.Replace(CallbackData.AcceptGameInvite, ""));

        await _bot.EditMessageText(chatId, messageId,
            LocalizationService.Get(lang, "invite_accepted"),
            cancellationToken: ct);

        await _bot.SendMessage(chatId,
            LocalizationService.Get(lang, "click_ready"),
            replyMarkup: _keyboard.GetReadyKeyboard(lang),
            cancellationToken: ct);

        // Notify the inviter that invitation was accepted
        var inviter = await userService.GetByIdAsync(inviterId);
        if (inviter != null)
        {
            var inviterLang = inviter.LanguageCode;
            var accepterUser = await userService.GetByIdAsync(userId);
            var accepterName = accepterUser?.FirstName ?? LocalizationService.Get(inviterLang, "player");

            await _bot.SendMessage(inviter.TelegramId,
                LocalizationService.Get(inviterLang, "friend_accepted_invite", accepterName),
                replyMarkup: _keyboard.GetReadyKeyboard(inviterLang),
                cancellationToken: ct);
        }
    }

    private async Task HandleDeclineGameInviteAsync(long chatId, int messageId, long telegramId, int userId,
        string lang, string data, IUserService userService, CancellationToken ct)
    {
        var inviterId = int.Parse(data.Replace(CallbackData.DeclineGameInvite, ""));

        await _bot.EditMessageText(chatId, messageId,
            LocalizationService.Get(lang, "invite_declined"),
            cancellationToken: ct);

        // Notify the inviter that the invitation was declined
        var inviter = await userService.GetByIdAsync(inviterId);
        if (inviter != null)
        {
            var inviterLang = inviter.LanguageCode;
            var declinerUser = await userService.GetByIdAsync(userId);
            var declinerName = declinerUser?.FirstName ?? LocalizationService.Get(inviterLang, "player");

            await _bot.SendMessage(inviter.TelegramId,
                LocalizationService.Get(inviterLang, "invite_was_declined", declinerName),
                replyMarkup: _keyboard.GetMainMenuReplyKeyboard(inviterLang),
                cancellationToken: ct);
        }
    }

    private async Task HandleSelectLanguageAsync(long chatId, int messageId, int userId, string data,
        IUserService userService, CancellationToken ct)
    {
        var languageCode = data.Replace(CallbackData.SelectLanguage, "");

        await userService.UpdateLanguageAsync(userId, languageCode);

        var (flag, name) = LocalizationService.GetLanguageInfo(languageCode);

        await _bot.EditMessageText(chatId, messageId,
            LocalizationService.Get(languageCode, "language_changed", flag, name),
            cancellationToken: ct);

        await SendProfileMenu(chatId, languageCode, ct);
    }
}
