namespace Victorina.Bot.Constants;

public static class BotCommands
{
    public const string Start = "/start";
    public const string Help = "/help";
    public const string Menu = "/menu";
}

public static class CallbackData
{
    // Главное меню
    public const string Play = "play";
    public const string QuickGame = "quick_game";
    public const string PlayWithFriend = "play_friend";
    public const string Statistics = "stats";
    public const string Friends = "friends";
    public const string BackToMenu = "back_menu";

    // Друзья
    public const string AddFriend = "add_friend";
    public const string FriendRequests = "friend_requests";
    public const string FriendsList = "friends_list";
    public const string AcceptFriend = "accept_friend_";
    public const string RejectFriend = "reject_friend_";
    public const string InviteFriend = "invite_friend_";

    // Игра
    public const string Ready = "ready";
    public const string Answer = "answer_";
    public const string CancelGame = "cancel_game";
    public const string AcceptGameInvite = "accept_invite_";
    public const string DeclineGameInvite = "decline_invite_";

    // Polling
    public const string CheckGame = "check_game";
    public const string CheckOpponent = "check_opponent";

    // Профиль
    public const string SelectLanguage = "lang_";
    public const string BackToProfile = "back_profile";

    // Категории
    public const string SelectCategory = "cat_";
    public const string SelectCategoryForFriend = "catf_";

    // Category Groups
    public const string SelectCategoryGroup = "grp_";
    public const string SelectCategoryGroupForFriend = "grpf_";
}

public static class BotMessages
{
    public const string Welcome = "Добро пожаловать в Викторину!\n\nИграйте с друзьями или случайными соперниками, отвечайте на вопросы быстрее и точнее!";

    public const string MainMenu = "Главное меню";

    public const string SearchingOpponent = "Ищем соперника...\n\nНажмите кнопку ниже, чтобы проверить статус.";

    public const string WaitingForReady = "Соперник найден!\n\n{0} vs {1}\n\nНажмите 'Готов' чтобы начать игру.";

    public const string GameStarting = "Игра начинается! Приготовьтесь...";

    public const string QuestionTemplate = "Вопрос {0}/{1}\n\n{2}";

    public const string CorrectAnswer = "Правильно!";
    public const string WrongAnswer = "Неверно!";

    public const string WaitingForOpponent = "Ожидаем ответ соперника...";

    public const string YouWon = "Вы победили! {0}\n\nВаш результат: {1} правильных, время: {2}\nСоперник: {3} правильных, время: {4}";
    public const string YouLost = "Вы проиграли. {0}\n\nВаш результат: {1} правильных, время: {2}\nСоперник: {3} правильных, время: {4}";
    public const string Draw = "Ничья!\n\nВаш результат: {0} правильных, время: {1}\nСоперник: {2} правильных, время: {3}";

    public const string NoFriends = "У вас пока нет друзей.\nНажмите 'Добавить друга' чтобы найти игроков.";
    public const string EnterUsername = "Введите имя пользователя или номер телефона для поиска:";
    public const string UserNotFound = "Пользователь не найден.";
    public const string FriendRequestSent = "Запрос в друзья отправлен!";
    public const string AlreadyFriends = "Вы уже друзья!";

    public const string NoPendingRequests = "Нет входящих запросов в друзья.";
    public const string FriendRequestAccepted = "Вы приняли запрос в друзья!";
    public const string FriendRequestRejected = "Запрос отклонён.";

    public const string GameInviteSent = "Приглашение отправлено! Ожидаем ответа...";
    public const string GameInviteReceived = "Вас приглашает в игру {0}!";
}
