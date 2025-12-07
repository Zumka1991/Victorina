namespace Victorina.Bot.Services;

public static class LocalizationService
{
    // Supported languages: ru, hi, pt, fa, de, uz, en
    private static readonly Dictionary<string, Dictionary<string, string>> Translations = new()
    {
        ["ru"] = new Dictionary<string, string>
        {
            // Welcome & Menu
            ["welcome"] = "🎯 *Викторина*\n\nВыберите действие:",
            ["select_country"] = "🌍 *Добро пожаловать!*\n\nPlease select your country:",
            ["play_menu"] = "🎮 *Выберите режим игры:*",
            ["profile_menu"] = "👤 *Мой профиль*\n\nВыберите раздел:",
            ["friends_menu"] = "👥 *Друзья*\n\nВыберите действие:",

            // Buttons
            ["btn_play"] = "🎮 Играть",
            ["btn_quick_game"] = "⚡ Быстрая игра",
            ["btn_play_with_friend"] = "👤 Играть с другом",
            ["btn_profile"] = "👤 Мой профиль",
            ["btn_statistics"] = "📊 Статистика",
            ["btn_language"] = "🌐 Язык",
            ["btn_leaders"] = "🏆 Лидеры",
            ["btn_friends"] = "👥 Друзья",
            ["btn_my_friends"] = "📋 Мои друзья",
            ["btn_add_friend"] = "➕ Добавить друга",
            ["btn_requests"] = "📩 Запросы",
            ["btn_back"] = "🔙 Назад",
            ["btn_back_to_profile"] = "🔙 В профиль",
            ["btn_cancel"] = "❌ Отмена",
            ["btn_leave_game"] = "❌ Покинуть игру",
            ["btn_ready"] = "✅ Готов",
            ["btn_check_opponent"] = "🔄 Проверить",
            ["btn_help"] = "❓ Помощь",
            ["btn_accept"] = "✅ Принять",
            ["btn_reject"] = "❌ Отклонить",
            ["btn_to_menu"] = "🏠 В меню",

            // Game
            ["searching_opponent"] = "🔍 *Ищем соперника...*\n\nПодождите, пока кто-то присоединится.",
            ["searching_category"] = "🔍 *Ищем соперника в выбранной категории...*",
            ["opponent_found"] = "🎮 *Соперник найден!*",
            ["select_category"] = "📚 *Выберите категорию:*",
            ["select_category_friend"] = "📚 *Выберите категорию для игры с другом:*",
            ["any_category"] = "🎲 Любая категория",
            ["game_starting"] = "🚀 *Игра начинается!*",
            ["waiting_opponent"] = "⏳ Ожидаем соперника...",
            ["waiting_ready"] = "✅ Вы готовы! Ожидаем соперника...",
            ["question"] = "❓ *Вопрос {0}/{1}*",
            ["question_label"] = "Вопрос",
            ["correct"] = "✅ Правильно!",
            ["incorrect"] = "❌ Неверно!",
            ["correct_answer"] = "Правильный ответ: *{0}*",
            ["your_time"] = "⏱ Ваше время: {0} сек",
            ["time_up"] = "⏱ *Время вышло!*\n\nПравильный ответ: *{0}*",
            ["opponent_answering"] = "⏳ Ожидаем ответ соперника...",
            ["opponent_still_answering"] = "⏳ Соперник ещё отвечает...",

            // Results
            ["you_won"] = "🏆 Вы победили!",
            ["you_lost"] = "😔 Вы проиграли",
            ["draw"] = "🤝 Ничья!",
            ["your_result"] = "📊 *Ваш результат:*",
            ["correct_answers"] = "✅ Правильных: {0}",
            ["time_spent"] = "⏱ Время: {0} сек",
            ["opponent_result"] = "📊 *Соперник:* {0} {1}",
            ["win_by_answers"] = "по количеству правильных ответов",
            ["win_by_time"] = "по времени",

            // Statistics
            ["your_statistics"] = "📊 *Ваша статистика*\n\n🎮 Игр сыграно: *{0}*\n🏆 Побед: *{1}*\n📈 Процент побед: *{2}%*\n✅ Правильных ответов: *{3}*",

            // Leaderboard
            ["leaderboard"] = "🏆 *Таблица лидеров*",
            ["leaderboard_empty"] = "🏆 *Таблица лидеров*\n\nПока нет игроков с завершёнными играми.\n\nСыграйте первую игру!",
            ["your_position"] = "📍 *Ваша позиция:* #{0}",
            ["play_to_rank"] = "📍 Сыграйте игру, чтобы попасть в рейтинг!",
            ["wins"] = "побед",
            ["games"] = "игр",

            // Friends
            ["no_friends"] = "😔 У вас пока нет друзей.\n\nНажмите «Добавить друга» чтобы найти игроков!",
            ["select_friend"] = "👤 *Выберите друга для игры:*",
            ["no_friends_for_game"] = "😔 У вас пока нет друзей.\n\nСначала добавьте друзей в разделе «Друзья»!",
            ["friend_search"] = "🔍 Enter friend's @username to invite (example: @ivan or ivan):",
            ["friend_not_found"] = "❌ Пользователь не найден.",
            ["already_friends"] = "👥 Вы уже друзья!",
            ["friend_request_sent"] = "✅ Запрос в друзья отправлен!",
            ["request_exists"] = "⚠️ Запрос уже существует.",
            ["game_invitation_sent"] = "✅ Invitation sent!",
            ["game_invite_from"] = "🎮 {0} is inviting you to play! Choose a category:",
            ["you_in_game"] = "⚠️ You already have an active game.",
            ["opponent_in_game"] = "⚠️ This player is already in a game. Try later.",
            ["game_invitation_sent"] = "✅ Taklif yuborildi!",
            ["game_invite_from"] = "🎮 {0} sizni o'yinga taklif qilmoqda! Kategoriya tanlang:",
            ["you_in_game"] = "⚠️ Sizda allaqachon faol o'yin bor.",
            ["opponent_in_game"] = "⚠️ Bu o'yinchi allaqachon o'yinda. Keyinroq urinib ko'ring.",
            ["game_invitation_sent"] = "✅ Einladung gesendet!",
            ["game_invite_from"] = "🎮 {0} lädt dich zum Spielen ein! Wähle eine Kategorie:",
            ["you_in_game"] = "⚠️ Du hast bereits ein aktives Spiel.",
            ["opponent_in_game"] = "⚠️ Dieser Spieler ist bereits im Spiel. Versuche es später.",
            ["game_invitation_sent"] = "✅ دعوت ارسال شد!",
            ["game_invite_from"] = "🎮 {0} شما را به بازی دعوت می‌کند! دسته را انتخاب کنید:",
            ["you_in_game"] = "⚠️ شما قبلاً یک بازی فعال دارید.",
            ["opponent_in_game"] = "⚠️ این بازیکن در حال بازی است. بعداً امتحان کنید.",
            ["game_invitation_sent"] = "✅ Convite enviado!",
            ["game_invite_from"] = "🎮 {0} está convidando você para jogar! Escolha a categoria:",
            ["you_in_game"] = "⚠️ Você já tem um jogo ativo.",
            ["opponent_in_game"] = "⚠️ Este jogador já está em jogo. Tente mais tarde.",
            ["game_invitation_sent"] = "✅ निमंत्रण भेजा गया!",
            ["game_invite_from"] = "🎮 {0} आपको खेलने के लिए आमंत्रित कर रहा है! श्रेणी चुनें:",
            ["you_in_game"] = "⚠️ आपका पहले से एक सक्रिय गेम है।",
            ["opponent_in_game"] = "⚠️ यह खिलाड़ी पहले से खेल में है। बाद में कोशिश करें।",
            ["game_invitation_sent"] = "✅ Приглашение отправлено!",
            ["game_invite_from"] = "🎮 {0} приглашает вас сыграть! Выберите категорию:",
            ["you_in_game"] = "⚠️ У вас уже есть активная игра.",
            ["opponent_in_game"] = "⚠️ Этот игрок уже в игре. Попробуйте позже.",
            ["new_friend_request"] = "📩 У вас новый запрос в друзья!",
            ["no_requests"] = "📭 Нет входящих запросов в друзья.",
            ["incoming_requests"] = "📩 *Входящие запросы:*",
            ["friend_accepted"] = "✅ Вы приняли запрос в друзья!",
            ["friend_rejected"] = "❌ Запрос отклонён.",
            ["accept_failed"] = "❌ Не удалось принять запрос.",

            // Game invites
            ["invite_sent"] = "📨 Приглашение отправлено!",
            ["waiting_response"] = "Ожидаем ответа...",
            ["click_ready"] = "Нажмите «Готов» когда друг примет приглашение.",
            ["game_invite"] = "🎮 *{0}* приглашает вас в игру!",
            ["category_info"] = "\n📚 Категория: *{0}*",

            // Language
            ["language_selection"] = "🌐 *Выбор языка*\n\nТекущий язык: {0} {1}\n\nВыберите язык интерфейса:",
            ["language_changed"] = "✅ Язык изменён!\n\n{0} {1}",

            // Game cancellation
            ["game_cancelled"] = "❌ Игра отменена.",
            ["opponent_left"] = "😔 Соперник покинул игру.",
            ["opponent_cancelled"] = "😔 Соперник отменил игру.",
            ["active_game_exists"] = "⚠️ У вас уже есть активная игра!",
            ["game_not_found"] = "❌ Игра не найдена.",

            // Help
            ["help"] = "🎯 *Викторина* — игра, где вы соревнуетесь с друзьями!\n\n🎮 *Как играть:*\n1. Нажмите «Играть»\n2. Выберите быструю игру или играйте с другом\n3. Отвечайте на вопросы быстрее соперника!\n\n🏆 Побеждает тот, кто даст больше правильных ответов. При равенстве — кто быстрее!",

            // Category Groups
            ["category_groups"] = "📁 *Разделы категорий:*",
            ["group_general"] = "📚 Общие",
            ["group_special"] = "⭐ Специальные",
            ["group_popular"] = "🔥 Популярные",
            ["group_my"] = "👤 Мои категории",
            ["group_all"] = "🎲 Любая категория",
            ["no_categories_found"] = "😔 В этом разделе пока нет категорий.",

            // Misc
            ["player"] = "Игрок",
        },

        ["hi"] = new Dictionary<string, string>
        {
            // Welcome & Menu
            ["welcome"] = "🎯 *क्विज़ गेम*\n\nएक क्रिया चुनें:",
            ["select_country"] = "🌍 *स्वागत है!*\n\nPlease select your country:",
            ["play_menu"] = "🎮 *गेम मोड चुनें:*",
            ["profile_menu"] = "👤 *मेरी प्रोफ़ाइल*\n\nसेक्शन चुनें:",
            ["friends_menu"] = "👥 *दोस्त*\n\nक्रिया चुनें:",

            // Buttons
            ["btn_play"] = "🎮 खेलें",
            ["btn_quick_game"] = "⚡ क्विक गेम",
            ["btn_play_with_friend"] = "👤 दोस्त के साथ खेलें",
            ["btn_profile"] = "👤 मेरी प्रोफ़ाइल",
            ["btn_statistics"] = "📊 आंकड़े",
            ["btn_language"] = "🌐 भाषा",
            ["btn_leaders"] = "🏆 लीडर्स",
            ["btn_friends"] = "👥 दोस्त",
            ["btn_my_friends"] = "📋 मेरे दोस्त",
            ["btn_add_friend"] = "➕ दोस्त जोड़ें",
            ["btn_requests"] = "📩 अनुरोध",
            ["btn_back"] = "🔙 वापस",
            ["btn_back_to_profile"] = "🔙 प्रोफ़ाइल पर",
            ["btn_cancel"] = "❌ रद्द करें",
            ["btn_leave_game"] = "❌ गेम छोड़ें",
            ["btn_ready"] = "✅ तैयार",
            ["btn_check_opponent"] = "🔄 जांचें",
            ["btn_help"] = "❓ मदद",
            ["btn_accept"] = "✅ स्वीकार करें",
            ["btn_reject"] = "❌ अस्वीकार",
            ["btn_to_menu"] = "🏠 मेनू में",

            // Game
            ["searching_opponent"] = "🔍 *प्रतिद्वंद्वी खोज रहे हैं...*\n\nकृपया प्रतीक्षा करें।",
            ["searching_category"] = "🔍 *इस श्रेणी में प्रतिद्वंद्वी खोज रहे हैं...*",
            ["opponent_found"] = "🎮 *प्रतिद्वंद्वी मिल गया!*",
            ["select_category"] = "📚 *श्रेणी चुनें:*",
            ["select_category_friend"] = "📚 *दोस्त के साथ खेलने के लिए श्रेणी चुनें:*",
            ["any_category"] = "🎲 कोई भी श्रेणी",
            ["game_starting"] = "🚀 *गेम शुरू हो रहा है!*",
            ["waiting_opponent"] = "⏳ प्रतिद्वंद्वी की प्रतीक्षा...",
            ["waiting_ready"] = "✅ आप तैयार हैं! प्रतिद्वंद्वी की प्रतीक्षा...",
            ["question"] = "❓ *सवाल {0}/{1}*",
            ["question_label"] = "सवाल",
            ["correct"] = "✅ सही!",
            ["incorrect"] = "❌ गलत!",
            ["correct_answer"] = "सही उत्तर: *{0}*",
            ["your_time"] = "⏱ आपका समय: {0} सेकंड",
            ["time_up"] = "⏱ *समय समाप्त!*\n\nसही उत्तर: *{0}*",
            ["opponent_answering"] = "⏳ प्रतिद्वंद्वी के जवाब की प्रतीक्षा...",
            ["opponent_still_answering"] = "⏳ प्रतिद्वंद्वी अभी भी जवाब दे रहा है...",

            // Results
            ["you_won"] = "🏆 आप जीते!",
            ["you_lost"] = "😔 आप हारे",
            ["draw"] = "🤝 बराबरी!",
            ["your_result"] = "📊 *आपका परिणाम:*",
            ["correct_answers"] = "✅ सही: {0}",
            ["time_spent"] = "⏱ समय: {0} सेकंड",
            ["opponent_result"] = "📊 *प्रतिद्वंद्वी:* {0} {1}",
            ["win_by_answers"] = "सही उत्तरों से",
            ["win_by_time"] = "समय से",

            // Statistics
            ["your_statistics"] = "📊 *आपके आंकड़े*\n\n🎮 खेले गए गेम: *{0}*\n🏆 जीत: *{1}*\n📈 जीत दर: *{2}%*\n✅ सही उत्तर: *{3}*",

            // Leaderboard
            ["leaderboard"] = "🏆 *लीडरबोर्ड*",
            ["leaderboard_empty"] = "🏆 *लीडरबोर्ड*\n\nअभी तक कोई खिलाड़ी नहीं।\n\nअपना पहला गेम खेलें!",
            ["your_position"] = "📍 *आपकी स्थिति:* #{0}",
            ["play_to_rank"] = "📍 रैंकिंग में आने के लिए गेम खेलें!",
            ["wins"] = "जीत",
            ["games"] = "गेम",

            // Friends
            ["no_friends"] = "😔 अभी तक कोई दोस्त नहीं।\n\n«दोस्त जोड़ें» पर क्लिक करें!",
            ["select_friend"] = "👤 *खेलने के लिए दोस्त चुनें:*",
            ["no_friends_for_game"] = "😔 अभी तक कोई दोस्त नहीं।\n\nपहले «दोस्त» सेक्शन में दोस्त जोड़ें!",
            ["friend_search"] = "🔍 Enter friend's @username to invite (example: @ivan or ivan):",
            ["friend_not_found"] = "❌ उपयोगकर्ता नहीं मिला।",
            ["already_friends"] = "👥 आप पहले से दोस्त हैं!",
            ["friend_request_sent"] = "✅ दोस्ती का अनुरोध भेजा गया!",
            ["request_exists"] = "⚠️ अनुरोध पहले से मौजूद है।",
            ["game_invitation_sent"] = "✅ Invitation sent!",
            ["game_invite_from"] = "🎮 {0} is inviting you to play! Choose a category:",
            ["you_in_game"] = "⚠️ You already have an active game.",
            ["opponent_in_game"] = "⚠️ This player is already in a game. Try later.",
            ["game_invitation_sent"] = "✅ Taklif yuborildi!",
            ["game_invite_from"] = "🎮 {0} sizni o'yinga taklif qilmoqda! Kategoriya tanlang:",
            ["you_in_game"] = "⚠️ Sizda allaqachon faol o'yin bor.",
            ["opponent_in_game"] = "⚠️ Bu o'yinchi allaqachon o'yinda. Keyinroq urinib ko'ring.",
            ["game_invitation_sent"] = "✅ Einladung gesendet!",
            ["game_invite_from"] = "🎮 {0} lädt dich zum Spielen ein! Wähle eine Kategorie:",
            ["you_in_game"] = "⚠️ Du hast bereits ein aktives Spiel.",
            ["opponent_in_game"] = "⚠️ Dieser Spieler ist bereits im Spiel. Versuche es später.",
            ["game_invitation_sent"] = "✅ دعوت ارسال شد!",
            ["game_invite_from"] = "🎮 {0} شما را به بازی دعوت می‌کند! دسته را انتخاب کنید:",
            ["you_in_game"] = "⚠️ شما قبلاً یک بازی فعال دارید.",
            ["opponent_in_game"] = "⚠️ این بازیکن در حال بازی است. بعداً امتحان کنید.",
            ["game_invitation_sent"] = "✅ Convite enviado!",
            ["game_invite_from"] = "🎮 {0} está convidando você para jogar! Escolha a categoria:",
            ["you_in_game"] = "⚠️ Você já tem um jogo ativo.",
            ["opponent_in_game"] = "⚠️ Este jogador já está em jogo. Tente mais tarde.",
            ["game_invitation_sent"] = "✅ निमंत्रण भेजा गया!",
            ["game_invite_from"] = "🎮 {0} आपको खेलने के लिए आमंत्रित कर रहा है! श्रेणी चुनें:",
            ["you_in_game"] = "⚠️ आपका पहले से एक सक्रिय गेम है।",
            ["opponent_in_game"] = "⚠️ यह खिलाड़ी पहले से खेल में है। बाद में कोशिश करें।",
            ["game_invitation_sent"] = "✅ Приглашение отправлено!",
            ["game_invite_from"] = "🎮 {0} приглашает вас сыграть! Выберите категорию:",
            ["you_in_game"] = "⚠️ У вас уже есть активная игра.",
            ["opponent_in_game"] = "⚠️ Этот игрок уже в игре. Попробуйте позже.",
            ["new_friend_request"] = "📩 आपको नया दोस्ती का अनुरोध मिला है!",
            ["no_requests"] = "📭 कोई दोस्ती का अनुरोध नहीं।",
            ["incoming_requests"] = "📩 *आने वाले अनुरोध:*",
            ["friend_accepted"] = "✅ आपने दोस्ती का अनुरोध स्वीकार किया!",
            ["friend_rejected"] = "❌ अनुरोध अस्वीकृत।",
            ["accept_failed"] = "❌ अनुरोध स्वीकार करने में विफल।",

            // Game invites
            ["invite_sent"] = "📨 निमंत्रण भेजा गया!",
            ["waiting_response"] = "जवाब की प्रतीक्षा...",
            ["click_ready"] = "जब दोस्त निमंत्रण स्वीकार करे तो «तैयार» क्लिक करें।",
            ["game_invite"] = "🎮 *{0}* ने आपको खेलने के लिए आमंत्रित किया!",
            ["category_info"] = "\n📚 श्रेणी: *{0}*",

            // Language
            ["language_selection"] = "🌐 *भाषा चुनें*\n\nवर्तमान भाषा: {0} {1}\n\nइंटरफ़ेस भाषा चुनें:",
            ["language_changed"] = "✅ भाषा बदल गई!\n\n{0} {1}",

            // Game cancellation
            ["game_cancelled"] = "❌ गेम रद्द।",
            ["opponent_left"] = "😔 प्रतिद्वंद्वी ने गेम छोड़ दिया।",
            ["opponent_cancelled"] = "😔 प्रतिद्वंद्वी ने गेम रद्द कर दिया।",
            ["active_game_exists"] = "⚠️ आपके पास पहले से एक सक्रिय गेम है!",
            ["game_not_found"] = "❌ गेम नहीं मिला।",

            // Help
            ["help"] = "🎯 *क्विज़* — एक गेम जहाँ आप दोस्तों के साथ प्रतिस्पर्धा करते हैं!\n\n🎮 *कैसे खेलें:*\n1. «खेलें» क्लिक करें\n2. क्विक गेम चुनें या दोस्त के साथ खेलें\n3. प्रतिद्वंद्वी से तेज़ जवाब दें!\n\n🏆 सबसे अधिक सही उत्तर देने वाला जीतता है। बराबरी पर - सबसे तेज़!",

            // Category Groups
            ["category_groups"] = "📁 *श्रेणी अनुभाग:*",
            ["group_general"] = "📚 सामान्य",
            ["group_special"] = "⭐ विशेष",
            ["group_popular"] = "🔥 लोकप्रिय",
            ["group_my"] = "👤 मेरी श्रेणियाँ",
            ["group_all"] = "🎲 कोई भी श्रेणी",
            ["no_categories_found"] = "😔 इस अनुभाग में अभी कोई श्रेणी नहीं है।",

            // Misc
            ["player"] = "खिलाड़ी",
        },

        ["pt"] = new Dictionary<string, string>
        {
            // Welcome & Menu
            ["welcome"] = "🎯 *Quiz*\n\nEscolha uma ação:",
            ["select_country"] = "🌍 *Bem-vindo!*\n\nPlease select your country:",
            ["play_menu"] = "🎮 *Selecione o modo de jogo:*",
            ["profile_menu"] = "👤 *Meu Perfil*\n\nSelecione uma seção:",
            ["friends_menu"] = "👥 *Amigos*\n\nSelecione uma ação:",

            // Buttons
            ["btn_play"] = "🎮 Jogar",
            ["btn_quick_game"] = "⚡ Jogo Rápido",
            ["btn_play_with_friend"] = "👤 Jogar com Amigo",
            ["btn_profile"] = "👤 Meu Perfil",
            ["btn_statistics"] = "📊 Estatísticas",
            ["btn_language"] = "🌐 Idioma",
            ["btn_leaders"] = "🏆 Líderes",
            ["btn_friends"] = "👥 Amigos",
            ["btn_my_friends"] = "📋 Meus Amigos",
            ["btn_add_friend"] = "➕ Adicionar Amigo",
            ["btn_requests"] = "📩 Solicitações",
            ["btn_back"] = "🔙 Voltar",
            ["btn_back_to_profile"] = "🔙 Ao Perfil",
            ["btn_cancel"] = "❌ Cancelar",
            ["btn_leave_game"] = "❌ Sair do Jogo",
            ["btn_ready"] = "✅ Pronto",
            ["btn_check_opponent"] = "🔄 Verificar",
            ["btn_help"] = "❓ Ajuda",
            ["btn_accept"] = "✅ Aceitar",
            ["btn_reject"] = "❌ Rejeitar",
            ["btn_to_menu"] = "🏠 Menu",

            // Game
            ["searching_opponent"] = "🔍 *Procurando oponente...*\n\nAguarde enquanto alguém entra.",
            ["searching_category"] = "🔍 *Procurando oponente nesta categoria...*",
            ["opponent_found"] = "🎮 *Oponente encontrado!*",
            ["select_category"] = "📚 *Selecione uma categoria:*",
            ["select_category_friend"] = "📚 *Selecione uma categoria para jogar com amigo:*",
            ["any_category"] = "🎲 Qualquer categoria",
            ["game_starting"] = "🚀 *O jogo está começando!*",
            ["waiting_opponent"] = "⏳ Aguardando oponente...",
            ["waiting_ready"] = "✅ Você está pronto! Aguardando oponente...",
            ["question"] = "❓ *Pergunta {0}/{1}*",
            ["question_label"] = "Pergunta",
            ["correct"] = "✅ Correto!",
            ["incorrect"] = "❌ Incorreto!",
            ["correct_answer"] = "Resposta correta: *{0}*",
            ["your_time"] = "⏱ Seu tempo: {0} seg",
            ["time_up"] = "⏱ *Tempo esgotado!*\n\nResposta correta: *{0}*",
            ["opponent_answering"] = "⏳ Aguardando resposta do oponente...",
            ["opponent_still_answering"] = "⏳ Oponente ainda está respondendo...",

            // Results
            ["you_won"] = "🏆 Você venceu!",
            ["you_lost"] = "😔 Você perdeu",
            ["draw"] = "🤝 Empate!",
            ["your_result"] = "📊 *Seu resultado:*",
            ["correct_answers"] = "✅ Corretas: {0}",
            ["time_spent"] = "⏱ Tempo: {0} seg",
            ["opponent_result"] = "📊 *Oponente:* {0} {1}",
            ["win_by_answers"] = "por respostas corretas",
            ["win_by_time"] = "por tempo",

            // Statistics
            ["your_statistics"] = "📊 *Suas estatísticas*\n\n🎮 Jogos: *{0}*\n🏆 Vitórias: *{1}*\n📈 Taxa de vitória: *{2}%*\n✅ Respostas corretas: *{3}*",

            // Leaderboard
            ["leaderboard"] = "🏆 *Classificação*",
            ["leaderboard_empty"] = "🏆 *Classificação*\n\nAinda não há jogadores com jogos concluídos.\n\nJogue seu primeiro jogo!",
            ["your_position"] = "📍 *Sua posição:* #{0}",
            ["play_to_rank"] = "📍 Jogue para entrar no ranking!",
            ["wins"] = "vitórias",
            ["games"] = "jogos",

            // Friends
            ["no_friends"] = "😔 Você ainda não tem amigos.\n\nClique em «Adicionar Amigo» para encontrar jogadores!",
            ["select_friend"] = "👤 *Selecione um amigo para jogar:*",
            ["no_friends_for_game"] = "😔 Você ainda não tem amigos.\n\nPrimeiro adicione amigos na seção «Amigos»!",
            ["friend_search"] = "🔍 Enter friend's @username to invite (example: @ivan or ivan):",
            ["friend_not_found"] = "❌ Usuário não encontrado.",
            ["already_friends"] = "👥 Vocês já são amigos!",
            ["friend_request_sent"] = "✅ Solicitação de amizade enviada!",
            ["request_exists"] = "⚠️ Solicitação já existe.",
            ["game_invitation_sent"] = "✅ Invitation sent!",
            ["game_invite_from"] = "🎮 {0} is inviting you to play! Choose a category:",
            ["you_in_game"] = "⚠️ You already have an active game.",
            ["opponent_in_game"] = "⚠️ This player is already in a game. Try later.",
            ["game_invitation_sent"] = "✅ Taklif yuborildi!",
            ["game_invite_from"] = "🎮 {0} sizni o'yinga taklif qilmoqda! Kategoriya tanlang:",
            ["you_in_game"] = "⚠️ Sizda allaqachon faol o'yin bor.",
            ["opponent_in_game"] = "⚠️ Bu o'yinchi allaqachon o'yinda. Keyinroq urinib ko'ring.",
            ["game_invitation_sent"] = "✅ Einladung gesendet!",
            ["game_invite_from"] = "🎮 {0} lädt dich zum Spielen ein! Wähle eine Kategorie:",
            ["you_in_game"] = "⚠️ Du hast bereits ein aktives Spiel.",
            ["opponent_in_game"] = "⚠️ Dieser Spieler ist bereits im Spiel. Versuche es später.",
            ["game_invitation_sent"] = "✅ دعوت ارسال شد!",
            ["game_invite_from"] = "🎮 {0} شما را به بازی دعوت می‌کند! دسته را انتخاب کنید:",
            ["you_in_game"] = "⚠️ شما قبلاً یک بازی فعال دارید.",
            ["opponent_in_game"] = "⚠️ این بازیکن در حال بازی است. بعداً امتحان کنید.",
            ["game_invitation_sent"] = "✅ Convite enviado!",
            ["game_invite_from"] = "🎮 {0} está convidando você para jogar! Escolha a categoria:",
            ["you_in_game"] = "⚠️ Você já tem um jogo ativo.",
            ["opponent_in_game"] = "⚠️ Este jogador já está em jogo. Tente mais tarde.",
            ["game_invitation_sent"] = "✅ निमंत्रण भेजा गया!",
            ["game_invite_from"] = "🎮 {0} आपको खेलने के लिए आमंत्रित कर रहा है! श्रेणी चुनें:",
            ["you_in_game"] = "⚠️ आपका पहले से एक सक्रिय गेम है।",
            ["opponent_in_game"] = "⚠️ यह खिलाड़ी पहले से खेल में है। बाद में कोशिश करें।",
            ["game_invitation_sent"] = "✅ Приглашение отправлено!",
            ["game_invite_from"] = "🎮 {0} приглашает вас сыграть! Выберите категорию:",
            ["you_in_game"] = "⚠️ У вас уже есть активная игра.",
            ["opponent_in_game"] = "⚠️ Этот игрок уже в игре. Попробуйте позже.",
            ["new_friend_request"] = "📩 Você tem uma nova solicitação de amizade!",
            ["no_requests"] = "📭 Sem solicitações de amizade.",
            ["incoming_requests"] = "📩 *Solicitações recebidas:*",
            ["friend_accepted"] = "✅ Você aceitou a solicitação de amizade!",
            ["friend_rejected"] = "❌ Solicitação rejeitada.",
            ["accept_failed"] = "❌ Falha ao aceitar solicitação.",

            // Game invites
            ["invite_sent"] = "📨 Convite enviado!",
            ["waiting_response"] = "Aguardando resposta...",
            ["click_ready"] = "Clique em «Pronto» quando o amigo aceitar o convite.",
            ["game_invite"] = "🎮 *{0}* te convidou para jogar!",
            ["category_info"] = "\n📚 Categoria: *{0}*",

            // Language
            ["language_selection"] = "🌐 *Seleção de idioma*\n\nIdioma atual: {0} {1}\n\nSelecione o idioma da interface:",
            ["language_changed"] = "✅ Idioma alterado!\n\n{0} {1}",

            // Game cancellation
            ["game_cancelled"] = "❌ Jogo cancelado.",
            ["opponent_left"] = "😔 Oponente saiu do jogo.",
            ["opponent_cancelled"] = "😔 Oponente cancelou o jogo.",
            ["active_game_exists"] = "⚠️ Você já tem um jogo ativo!",
            ["game_not_found"] = "❌ Jogo não encontrado.",

            // Help
            ["help"] = "🎯 *Quiz* — um jogo onde você compete com amigos!\n\n🎮 *Como jogar:*\n1. Clique em «Jogar»\n2. Escolha jogo rápido ou jogue com um amigo\n3. Responda às perguntas mais rápido que seu oponente!\n\n🏆 Quem der mais respostas corretas vence. Em caso de empate — o mais rápido!",

            // Category Groups
            ["category_groups"] = "📁 *Seções de categorias:*",
            ["group_general"] = "📚 Gerais",
            ["group_special"] = "⭐ Especiais",
            ["group_popular"] = "🔥 Populares",
            ["group_my"] = "👤 Minhas categorias",
            ["group_all"] = "🎲 Qualquer categoria",
            ["no_categories_found"] = "😔 Ainda não há categorias nesta seção.",

            // Misc
            ["player"] = "Jogador",
        },

        ["fa"] = new Dictionary<string, string>
        {
            // Welcome & Menu
            ["welcome"] = "🎯 *مسابقه*\n\nیک عمل انتخاب کنید:",
            ["select_country"] = "🌍 *خوش آمدید!*\n\nPlease select your country:",
            ["play_menu"] = "🎮 *حالت بازی را انتخاب کنید:*",
            ["profile_menu"] = "👤 *پروفایل من*\n\nبخش را انتخاب کنید:",
            ["friends_menu"] = "👥 *دوستان*\n\nعمل را انتخاب کنید:",

            // Buttons
            ["btn_play"] = "🎮 بازی",
            ["btn_quick_game"] = "⚡ بازی سریع",
            ["btn_play_with_friend"] = "👤 بازی با دوست",
            ["btn_profile"] = "👤 پروفایل من",
            ["btn_statistics"] = "📊 آمار",
            ["btn_language"] = "🌐 زبان",
            ["btn_leaders"] = "🏆 رتبه‌بندی",
            ["btn_friends"] = "👥 دوستان",
            ["btn_my_friends"] = "📋 دوستان من",
            ["btn_add_friend"] = "➕ افزودن دوست",
            ["btn_requests"] = "📩 درخواست‌ها",
            ["btn_back"] = "🔙 برگشت",
            ["btn_back_to_profile"] = "🔙 به پروفایل",
            ["btn_cancel"] = "❌ لغو",
            ["btn_leave_game"] = "❌ خروج از بازی",
            ["btn_ready"] = "✅ آماده",
            ["btn_check_opponent"] = "🔄 بررسی",
            ["btn_help"] = "❓ راهنما",
            ["btn_accept"] = "✅ پذیرش",
            ["btn_reject"] = "❌ رد",
            ["btn_to_menu"] = "🏠 منو",

            // Game
            ["searching_opponent"] = "🔍 *در حال جستجوی حریف...*\n\nلطفاً صبر کنید.",
            ["searching_category"] = "🔍 *در حال جستجوی حریف در این دسته...*",
            ["opponent_found"] = "🎮 *حریف پیدا شد!*",
            ["select_category"] = "📚 *دسته را انتخاب کنید:*",
            ["select_category_friend"] = "📚 *دسته برای بازی با دوست را انتخاب کنید:*",
            ["any_category"] = "🎲 هر دسته‌ای",
            ["game_starting"] = "🚀 *بازی شروع می‌شود!*",
            ["waiting_opponent"] = "⏳ در انتظار حریف...",
            ["waiting_ready"] = "✅ شما آماده‌اید! در انتظار حریف...",
            ["question"] = "❓ *سوال {0}/{1}*",
            ["question_label"] = "سوال",
            ["correct"] = "✅ درست!",
            ["incorrect"] = "❌ نادرست!",
            ["correct_answer"] = "پاسخ صحیح: *{0}*",
            ["your_time"] = "⏱ زمان شما: {0} ثانیه",
            ["time_up"] = "⏱ *زمان تمام شد!*\n\nپاسخ صحیح: *{0}*",
            ["opponent_answering"] = "⏳ در انتظار پاسخ حریف...",
            ["opponent_still_answering"] = "⏳ حریف هنوز پاسخ می‌دهد...",

            // Results
            ["you_won"] = "🏆 شما برنده شدید!",
            ["you_lost"] = "😔 شما باختید",
            ["draw"] = "🤝 مساوی!",
            ["your_result"] = "📊 *نتیجه شما:*",
            ["correct_answers"] = "✅ درست: {0}",
            ["time_spent"] = "⏱ زمان: {0} ثانیه",
            ["opponent_result"] = "📊 *حریف:* {0} {1}",
            ["win_by_answers"] = "با پاسخ‌های صحیح",
            ["win_by_time"] = "با زمان",

            // Statistics
            ["your_statistics"] = "📊 *آمار شما*\n\n🎮 بازی‌ها: *{0}*\n🏆 برد: *{1}*\n📈 نرخ برد: *{2}%*\n✅ پاسخ‌های صحیح: *{3}*",

            // Leaderboard
            ["leaderboard"] = "🏆 *رتبه‌بندی*",
            ["leaderboard_empty"] = "🏆 *رتبه‌بندی*\n\nهنوز بازیکنی با بازی تمام شده نیست.\n\nاولین بازی خود را انجام دهید!",
            ["your_position"] = "📍 *رتبه شما:* #{0}",
            ["play_to_rank"] = "📍 برای ورود به رتبه‌بندی بازی کنید!",
            ["wins"] = "برد",
            ["games"] = "بازی",

            // Friends
            ["no_friends"] = "😔 هنوز دوستی ندارید.\n\nروی «افزودن دوست» کلیک کنید!",
            ["select_friend"] = "👤 *دوستی برای بازی انتخاب کنید:*",
            ["no_friends_for_game"] = "😔 هنوز دوستی ندارید.\n\nابتدا در بخش «دوستان» دوست اضافه کنید!",
            ["friend_search"] = "🔍 Enter friend's @username to invite (example: @ivan or ivan):",
            ["friend_not_found"] = "❌ کاربر یافت نشد.",
            ["already_friends"] = "👥 شما قبلاً دوست هستید!",
            ["friend_request_sent"] = "✅ درخواست دوستی ارسال شد!",
            ["request_exists"] = "⚠️ درخواست قبلاً وجود دارد.",
            ["game_invitation_sent"] = "✅ Invitation sent!",
            ["game_invite_from"] = "🎮 {0} is inviting you to play! Choose a category:",
            ["you_in_game"] = "⚠️ You already have an active game.",
            ["opponent_in_game"] = "⚠️ This player is already in a game. Try later.",
            ["game_invitation_sent"] = "✅ Taklif yuborildi!",
            ["game_invite_from"] = "🎮 {0} sizni o'yinga taklif qilmoqda! Kategoriya tanlang:",
            ["you_in_game"] = "⚠️ Sizda allaqachon faol o'yin bor.",
            ["opponent_in_game"] = "⚠️ Bu o'yinchi allaqachon o'yinda. Keyinroq urinib ko'ring.",
            ["game_invitation_sent"] = "✅ Einladung gesendet!",
            ["game_invite_from"] = "🎮 {0} lädt dich zum Spielen ein! Wähle eine Kategorie:",
            ["you_in_game"] = "⚠️ Du hast bereits ein aktives Spiel.",
            ["opponent_in_game"] = "⚠️ Dieser Spieler ist bereits im Spiel. Versuche es später.",
            ["game_invitation_sent"] = "✅ دعوت ارسال شد!",
            ["game_invite_from"] = "🎮 {0} شما را به بازی دعوت می‌کند! دسته را انتخاب کنید:",
            ["you_in_game"] = "⚠️ شما قبلاً یک بازی فعال دارید.",
            ["opponent_in_game"] = "⚠️ این بازیکن در حال بازی است. بعداً امتحان کنید.",
            ["game_invitation_sent"] = "✅ Convite enviado!",
            ["game_invite_from"] = "🎮 {0} está convidando você para jogar! Escolha a categoria:",
            ["you_in_game"] = "⚠️ Você já tem um jogo ativo.",
            ["opponent_in_game"] = "⚠️ Este jogador já está em jogo. Tente mais tarde.",
            ["game_invitation_sent"] = "✅ निमंत्रण भेजा गया!",
            ["game_invite_from"] = "🎮 {0} आपको खेलने के लिए आमंत्रित कर रहा है! श्रेणी चुनें:",
            ["you_in_game"] = "⚠️ आपका पहले से एक सक्रिय गेम है।",
            ["opponent_in_game"] = "⚠️ यह खिलाड़ी पहले से खेल में है। बाद में कोशिश करें।",
            ["game_invitation_sent"] = "✅ Приглашение отправлено!",
            ["game_invite_from"] = "🎮 {0} приглашает вас сыграть! Выберите категорию:",
            ["you_in_game"] = "⚠️ У вас уже есть активная игра.",
            ["opponent_in_game"] = "⚠️ Этот игрок уже в игре. Попробуйте позже.",
            ["new_friend_request"] = "📩 شما درخواست دوستی جدید دارید!",
            ["no_requests"] = "📭 درخواست دوستی ندارید.",
            ["incoming_requests"] = "📩 *درخواست‌های دریافتی:*",
            ["friend_accepted"] = "✅ درخواست دوستی را پذیرفتید!",
            ["friend_rejected"] = "❌ درخواست رد شد.",
            ["accept_failed"] = "❌ پذیرش درخواست ناموفق بود.",

            // Game invites
            ["invite_sent"] = "📨 دعوت‌نامه ارسال شد!",
            ["waiting_response"] = "در انتظار پاسخ...",
            ["click_ready"] = "وقتی دوست دعوت را پذیرفت روی «آماده» کلیک کنید.",
            ["game_invite"] = "🎮 *{0}* شما را به بازی دعوت کرد!",
            ["category_info"] = "\n📚 دسته: *{0}*",

            // Language
            ["language_selection"] = "🌐 *انتخاب زبان*\n\nزبان فعلی: {0} {1}\n\nزبان رابط کاربری را انتخاب کنید:",
            ["language_changed"] = "✅ زبان تغییر کرد!\n\n{0} {1}",

            // Game cancellation
            ["game_cancelled"] = "❌ بازی لغو شد.",
            ["opponent_left"] = "😔 حریف بازی را ترک کرد.",
            ["opponent_cancelled"] = "😔 حریف بازی را لغو کرد.",
            ["active_game_exists"] = "⚠️ شما قبلاً یک بازی فعال دارید!",
            ["game_not_found"] = "❌ بازی یافت نشد.",

            // Help
            ["help"] = "🎯 *مسابقه* — بازی که با دوستان رقابت می‌کنید!\n\n🎮 *نحوه بازی:*\n1. روی «بازی» کلیک کنید\n2. بازی سریع یا بازی با دوست را انتخاب کنید\n3. سریع‌تر از حریف پاسخ دهید!\n\n🏆 کسی که بیشترین پاسخ صحیح را بدهد برنده است. در صورت تساوی — سریع‌ترین!",

            // Category Groups
            ["category_groups"] = "📁 *بخش‌های دسته‌بندی:*",
            ["group_general"] = "📚 عمومی",
            ["group_special"] = "⭐ ویژه",
            ["group_popular"] = "🔥 محبوب",
            ["group_my"] = "👤 دسته‌های من",
            ["group_all"] = "🎲 هر دسته‌ای",
            ["no_categories_found"] = "😔 هنوز دسته‌ای در این بخش وجود ندارد.",

            // Misc
            ["player"] = "بازیکن",
        },

        ["de"] = new Dictionary<string, string>
        {
            // Welcome & Menu
            ["welcome"] = "🎯 *Quiz*\n\nWähle eine Aktion:",
            ["select_country"] = "🌍 *Willkommen!*\n\nPlease select your country:",
            ["play_menu"] = "🎮 *Spielmodus wählen:*",
            ["profile_menu"] = "👤 *Mein Profil*\n\nBereich wählen:",
            ["friends_menu"] = "👥 *Freunde*\n\nAktion wählen:",

            // Buttons
            ["btn_play"] = "🎮 Spielen",
            ["btn_quick_game"] = "⚡ Schnelles Spiel",
            ["btn_play_with_friend"] = "👤 Mit Freund spielen",
            ["btn_profile"] = "👤 Mein Profil",
            ["btn_statistics"] = "📊 Statistik",
            ["btn_language"] = "🌐 Sprache",
            ["btn_leaders"] = "🏆 Rangliste",
            ["btn_friends"] = "👥 Freunde",
            ["btn_my_friends"] = "📋 Meine Freunde",
            ["btn_add_friend"] = "➕ Freund hinzufügen",
            ["btn_requests"] = "📩 Anfragen",
            ["btn_back"] = "🔙 Zurück",
            ["btn_back_to_profile"] = "🔙 Zum Profil",
            ["btn_cancel"] = "❌ Abbrechen",
            ["btn_leave_game"] = "❌ Spiel verlassen",
            ["btn_ready"] = "✅ Bereit",
            ["btn_check_opponent"] = "🔄 Prüfen",
            ["btn_help"] = "❓ Hilfe",
            ["btn_accept"] = "✅ Annehmen",
            ["btn_reject"] = "❌ Ablehnen",
            ["btn_to_menu"] = "🏠 Menü",

            // Game
            ["searching_opponent"] = "🔍 *Suche Gegner...*\n\nBitte warten.",
            ["searching_category"] = "🔍 *Suche Gegner in dieser Kategorie...*",
            ["opponent_found"] = "🎮 *Gegner gefunden!*",
            ["select_category"] = "📚 *Kategorie wählen:*",
            ["select_category_friend"] = "📚 *Kategorie für Spiel mit Freund wählen:*",
            ["any_category"] = "🎲 Beliebige Kategorie",
            ["game_starting"] = "🚀 *Das Spiel beginnt!*",
            ["waiting_opponent"] = "⏳ Warte auf Gegner...",
            ["waiting_ready"] = "✅ Du bist bereit! Warte auf Gegner...",
            ["question"] = "❓ *Frage {0}/{1}*",
            ["question_label"] = "Frage",
            ["correct"] = "✅ Richtig!",
            ["incorrect"] = "❌ Falsch!",
            ["correct_answer"] = "Richtige Antwort: *{0}*",
            ["your_time"] = "⏱ Deine Zeit: {0} Sek",
            ["time_up"] = "⏱ *Zeit abgelaufen!*\n\nRichtige Antwort: *{0}*",
            ["opponent_answering"] = "⏳ Warte auf Antwort des Gegners...",
            ["opponent_still_answering"] = "⏳ Gegner antwortet noch...",

            // Results
            ["you_won"] = "🏆 Du hast gewonnen!",
            ["you_lost"] = "😔 Du hast verloren",
            ["draw"] = "🤝 Unentschieden!",
            ["your_result"] = "📊 *Dein Ergebnis:*",
            ["correct_answers"] = "✅ Richtig: {0}",
            ["time_spent"] = "⏱ Zeit: {0} Sek",
            ["opponent_result"] = "📊 *Gegner:* {0} {1}",
            ["win_by_answers"] = "durch richtige Antworten",
            ["win_by_time"] = "durch Zeit",

            // Statistics
            ["your_statistics"] = "📊 *Deine Statistik*\n\n🎮 Gespielte Spiele: *{0}*\n🏆 Siege: *{1}*\n📈 Siegquote: *{2}%*\n✅ Richtige Antworten: *{3}*",

            // Leaderboard
            ["leaderboard"] = "🏆 *Rangliste*",
            ["leaderboard_empty"] = "🏆 *Rangliste*\n\nNoch keine Spieler mit abgeschlossenen Spielen.\n\nSpiele dein erstes Spiel!",
            ["your_position"] = "📍 *Deine Position:* #{0}",
            ["play_to_rank"] = "📍 Spiele ein Spiel, um in die Rangliste zu kommen!",
            ["wins"] = "Siege",
            ["games"] = "Spiele",

            // Friends
            ["no_friends"] = "😔 Du hast noch keine Freunde.\n\nKlicke auf «Freund hinzufügen»!",
            ["select_friend"] = "👤 *Freund zum Spielen wählen:*",
            ["no_friends_for_game"] = "😔 Du hast noch keine Freunde.\n\nFüge zuerst Freunde im Bereich «Freunde» hinzu!",
            ["friend_search"] = "🔍 Enter friend's @username to invite (example: @ivan or ivan):",
            ["friend_not_found"] = "❌ Benutzer nicht gefunden.",
            ["already_friends"] = "👥 Ihr seid bereits Freunde!",
            ["friend_request_sent"] = "✅ Freundschaftsanfrage gesendet!",
            ["request_exists"] = "⚠️ Anfrage existiert bereits.",
            ["game_invitation_sent"] = "✅ Invitation sent!",
            ["game_invite_from"] = "🎮 {0} is inviting you to play! Choose a category:",
            ["you_in_game"] = "⚠️ You already have an active game.",
            ["opponent_in_game"] = "⚠️ This player is already in a game. Try later.",
            ["game_invitation_sent"] = "✅ Taklif yuborildi!",
            ["game_invite_from"] = "🎮 {0} sizni o'yinga taklif qilmoqda! Kategoriya tanlang:",
            ["you_in_game"] = "⚠️ Sizda allaqachon faol o'yin bor.",
            ["opponent_in_game"] = "⚠️ Bu o'yinchi allaqachon o'yinda. Keyinroq urinib ko'ring.",
            ["game_invitation_sent"] = "✅ Einladung gesendet!",
            ["game_invite_from"] = "🎮 {0} lädt dich zum Spielen ein! Wähle eine Kategorie:",
            ["you_in_game"] = "⚠️ Du hast bereits ein aktives Spiel.",
            ["opponent_in_game"] = "⚠️ Dieser Spieler ist bereits im Spiel. Versuche es später.",
            ["game_invitation_sent"] = "✅ دعوت ارسال شد!",
            ["game_invite_from"] = "🎮 {0} شما را به بازی دعوت می‌کند! دسته را انتخاب کنید:",
            ["you_in_game"] = "⚠️ شما قبلاً یک بازی فعال دارید.",
            ["opponent_in_game"] = "⚠️ این بازیکن در حال بازی است. بعداً امتحان کنید.",
            ["game_invitation_sent"] = "✅ Convite enviado!",
            ["game_invite_from"] = "🎮 {0} está convidando você para jogar! Escolha a categoria:",
            ["you_in_game"] = "⚠️ Você já tem um jogo ativo.",
            ["opponent_in_game"] = "⚠️ Este jogador já está em jogo. Tente mais tarde.",
            ["game_invitation_sent"] = "✅ निमंत्रण भेजा गया!",
            ["game_invite_from"] = "🎮 {0} आपको खेलने के लिए आमंत्रित कर रहा है! श्रेणी चुनें:",
            ["you_in_game"] = "⚠️ आपका पहले से एक सक्रिय गेम है।",
            ["opponent_in_game"] = "⚠️ यह खिलाड़ी पहले से खेल में है। बाद में कोशिश करें।",
            ["game_invitation_sent"] = "✅ Приглашение отправлено!",
            ["game_invite_from"] = "🎮 {0} приглашает вас сыграть! Выберите категорию:",
            ["you_in_game"] = "⚠️ У вас уже есть активная игра.",
            ["opponent_in_game"] = "⚠️ Этот игрок уже в игре. Попробуйте позже.",
            ["new_friend_request"] = "📩 Du hast eine neue Freundschaftsanfrage!",
            ["no_requests"] = "📭 Keine Freundschaftsanfragen.",
            ["incoming_requests"] = "📩 *Eingehende Anfragen:*",
            ["friend_accepted"] = "✅ Du hast die Freundschaftsanfrage angenommen!",
            ["friend_rejected"] = "❌ Anfrage abgelehnt.",
            ["accept_failed"] = "❌ Annahme fehlgeschlagen.",

            // Game invites
            ["invite_sent"] = "📨 Einladung gesendet!",
            ["waiting_response"] = "Warte auf Antwort...",
            ["click_ready"] = "Klicke «Bereit» wenn der Freund die Einladung annimmt.",
            ["game_invite"] = "🎮 *{0}* lädt dich zum Spielen ein!",
            ["category_info"] = "\n📚 Kategorie: *{0}*",

            // Language
            ["language_selection"] = "🌐 *Sprache auswählen*\n\nAktuelle Sprache: {0} {1}\n\nWähle die Schnittstellensprache:",
            ["language_changed"] = "✅ Sprache geändert!\n\n{0} {1}",

            // Game cancellation
            ["game_cancelled"] = "❌ Spiel abgebrochen.",
            ["opponent_left"] = "😔 Gegner hat das Spiel verlassen.",
            ["opponent_cancelled"] = "😔 Gegner hat das Spiel abgebrochen.",
            ["active_game_exists"] = "⚠️ Du hast bereits ein aktives Spiel!",
            ["game_not_found"] = "❌ Spiel nicht gefunden.",

            // Help
            ["help"] = "🎯 *Quiz* — ein Spiel, in dem du gegen Freunde antrittst!\n\n🎮 *So spielst du:*\n1. Klicke auf «Spielen»\n2. Wähle schnelles Spiel oder spiele mit einem Freund\n3. Antworte schneller als dein Gegner!\n\n🏆 Wer die meisten richtigen Antworten gibt, gewinnt. Bei Gleichstand — der Schnellere!",

            // Category Groups
            ["category_groups"] = "📁 *Kategoriebereiche:*",
            ["group_general"] = "📚 Allgemein",
            ["group_special"] = "⭐ Speziell",
            ["group_popular"] = "🔥 Beliebt",
            ["group_my"] = "👤 Meine Kategorien",
            ["group_all"] = "🎲 Beliebige Kategorie",
            ["no_categories_found"] = "😔 In diesem Bereich gibt es noch keine Kategorien.",

            // Misc
            ["player"] = "Spieler",
        },

        ["uz"] = new Dictionary<string, string>
        {
            // Welcome & Menu
            ["welcome"] = "🎯 *Viktorina*\n\nAmalni tanlang:",
            ["select_country"] = "🌍 *Xush kelibsiz!*\n\nPlease select your country:",
            ["play_menu"] = "🎮 *O'yin rejimini tanlang:*",
            ["profile_menu"] = "👤 *Mening profilim*\n\nBo'limni tanlang:",
            ["friends_menu"] = "👥 *Do'stlar*\n\nAmalni tanlang:",

            // Buttons
            ["btn_play"] = "🎮 O'ynash",
            ["btn_quick_game"] = "⚡ Tez o'yin",
            ["btn_play_with_friend"] = "👤 Do'st bilan o'ynash",
            ["btn_profile"] = "👤 Mening profilim",
            ["btn_statistics"] = "📊 Statistika",
            ["btn_language"] = "🌐 Til",
            ["btn_leaders"] = "🏆 Liderlar",
            ["btn_friends"] = "👥 Do'stlar",
            ["btn_my_friends"] = "📋 Mening do'stlarim",
            ["btn_add_friend"] = "➕ Do'st qo'shish",
            ["btn_requests"] = "📩 So'rovlar",
            ["btn_back"] = "🔙 Orqaga",
            ["btn_back_to_profile"] = "🔙 Profilga",
            ["btn_cancel"] = "❌ Bekor qilish",
            ["btn_leave_game"] = "❌ O'yindan chiqish",
            ["btn_ready"] = "✅ Tayyor",
            ["btn_check_opponent"] = "🔄 Tekshirish",
            ["btn_help"] = "❓ Yordam",
            ["btn_accept"] = "✅ Qabul qilish",
            ["btn_reject"] = "❌ Rad etish",
            ["btn_to_menu"] = "🏠 Menyu",

            // Game
            ["searching_opponent"] = "🔍 *Raqib qidirilmoqda...*\n\nIltimos, kuting.",
            ["searching_category"] = "🔍 *Bu kategoriyada raqib qidirilmoqda...*",
            ["opponent_found"] = "🎮 *Raqib topildi!*",
            ["select_category"] = "📚 *Kategoriyani tanlang:*",
            ["select_category_friend"] = "📚 *Do'st bilan o'yin uchun kategoriya tanlang:*",
            ["any_category"] = "🎲 Har qanday kategoriya",
            ["game_starting"] = "🚀 *O'yin boshlanmoqda!*",
            ["waiting_opponent"] = "⏳ Raqib kutilmoqda...",
            ["waiting_ready"] = "✅ Siz tayyorsiz! Raqib kutilmoqda...",
            ["question"] = "❓ *Savol {0}/{1}*",
            ["question_label"] = "Savol",
            ["correct"] = "✅ To'g'ri!",
            ["incorrect"] = "❌ Noto'g'ri!",
            ["correct_answer"] = "To'g'ri javob: *{0}*",
            ["your_time"] = "⏱ Sizning vaqtingiz: {0} sek",
            ["time_up"] = "⏱ *Vaqt tugadi!*\n\nTo'g'ri javob: *{0}*",
            ["opponent_answering"] = "⏳ Raqib javobi kutilmoqda...",
            ["opponent_still_answering"] = "⏳ Raqib hali javob bermoqda...",

            // Results
            ["you_won"] = "🏆 Siz g'olib bo'ldingiz!",
            ["you_lost"] = "😔 Siz yutqazdingiz",
            ["draw"] = "🤝 Durrang!",
            ["your_result"] = "📊 *Sizning natijangiz:*",
            ["correct_answers"] = "✅ To'g'ri: {0}",
            ["time_spent"] = "⏱ Vaqt: {0} sek",
            ["opponent_result"] = "📊 *Raqib:* {0} {1}",
            ["win_by_answers"] = "to'g'ri javoblar bo'yicha",
            ["win_by_time"] = "vaqt bo'yicha",

            // Statistics
            ["your_statistics"] = "📊 *Sizning statistikangiz*\n\n🎮 O'yinlar: *{0}*\n🏆 G'alabalar: *{1}*\n📈 G'alaba foizi: *{2}%*\n✅ To'g'ri javoblar: *{3}*",

            // Leaderboard
            ["leaderboard"] = "🏆 *Liderlar jadvali*",
            ["leaderboard_empty"] = "🏆 *Liderlar jadvali*\n\nHali tugallangan o'yinlar yo'q.\n\nBirinchi o'yiningizni o'ynang!",
            ["your_position"] = "📍 *Sizning o'rningiz:* #{0}",
            ["play_to_rank"] = "📍 Reytingga kirish uchun o'ynang!",
            ["wins"] = "g'alaba",
            ["games"] = "o'yin",

            // Friends
            ["no_friends"] = "😔 Hali do'stlaringiz yo'q.\n\n«Do'st qo'shish» tugmasini bosing!",
            ["select_friend"] = "👤 *O'ynash uchun do'stni tanlang:*",
            ["no_friends_for_game"] = "😔 Hali do'stlaringiz yo'q.\n\nAvval «Do'stlar» bo'limida do'st qo'shing!",
            ["friend_search"] = "🔍 Enter friend's @username to invite (example: @ivan or ivan):",
            ["friend_not_found"] = "❌ Foydalanuvchi topilmadi.",
            ["already_friends"] = "👥 Siz allaqachon do'stsiz!",
            ["friend_request_sent"] = "✅ Do'stlik so'rovi yuborildi!",
            ["request_exists"] = "⚠️ So'rov allaqachon mavjud.",
            ["game_invitation_sent"] = "✅ Invitation sent!",
            ["game_invite_from"] = "🎮 {0} is inviting you to play! Choose a category:",
            ["you_in_game"] = "⚠️ You already have an active game.",
            ["opponent_in_game"] = "⚠️ This player is already in a game. Try later.",
            ["game_invitation_sent"] = "✅ Taklif yuborildi!",
            ["game_invite_from"] = "🎮 {0} sizni o'yinga taklif qilmoqda! Kategoriya tanlang:",
            ["you_in_game"] = "⚠️ Sizda allaqachon faol o'yin bor.",
            ["opponent_in_game"] = "⚠️ Bu o'yinchi allaqachon o'yinda. Keyinroq urinib ko'ring.",
            ["game_invitation_sent"] = "✅ Einladung gesendet!",
            ["game_invite_from"] = "🎮 {0} lädt dich zum Spielen ein! Wähle eine Kategorie:",
            ["you_in_game"] = "⚠️ Du hast bereits ein aktives Spiel.",
            ["opponent_in_game"] = "⚠️ Dieser Spieler ist bereits im Spiel. Versuche es später.",
            ["game_invitation_sent"] = "✅ دعوت ارسال شد!",
            ["game_invite_from"] = "🎮 {0} شما را به بازی دعوت می‌کند! دسته را انتخاب کنید:",
            ["you_in_game"] = "⚠️ شما قبلاً یک بازی فعال دارید.",
            ["opponent_in_game"] = "⚠️ این بازیکن در حال بازی است. بعداً امتحان کنید.",
            ["game_invitation_sent"] = "✅ Convite enviado!",
            ["game_invite_from"] = "🎮 {0} está convidando você para jogar! Escolha a categoria:",
            ["you_in_game"] = "⚠️ Você já tem um jogo ativo.",
            ["opponent_in_game"] = "⚠️ Este jogador já está em jogo. Tente mais tarde.",
            ["game_invitation_sent"] = "✅ निमंत्रण भेजा गया!",
            ["game_invite_from"] = "🎮 {0} आपको खेलने के लिए आमंत्रित कर रहा है! श्रेणी चुनें:",
            ["you_in_game"] = "⚠️ आपका पहले से एक सक्रिय गेम है।",
            ["opponent_in_game"] = "⚠️ यह खिलाड़ी पहले से खेल में है। बाद में कोशिश करें।",
            ["game_invitation_sent"] = "✅ Приглашение отправлено!",
            ["game_invite_from"] = "🎮 {0} приглашает вас сыграть! Выберите категорию:",
            ["you_in_game"] = "⚠️ У вас уже есть активная игра.",
            ["opponent_in_game"] = "⚠️ Этот игрок уже в игре. Попробуйте позже.",
            ["new_friend_request"] = "📩 Sizda yangi do'stlik so'rovi bor!",
            ["no_requests"] = "📭 Do'stlik so'rovlari yo'q.",
            ["incoming_requests"] = "📩 *Kelgan so'rovlar:*",
            ["friend_accepted"] = "✅ Siz do'stlik so'rovini qabul qildingiz!",
            ["friend_rejected"] = "❌ So'rov rad etildi.",
            ["accept_failed"] = "❌ So'rovni qabul qilib bo'lmadi.",

            // Game invites
            ["invite_sent"] = "📨 Taklif yuborildi!",
            ["waiting_response"] = "Javob kutilmoqda...",
            ["click_ready"] = "Do'st taklifni qabul qilganda «Tayyor» tugmasini bosing.",
            ["game_invite"] = "🎮 *{0}* sizni o'yinga taklif qildi!",
            ["category_info"] = "\n📚 Kategoriya: *{0}*",

            // Language
            ["language_selection"] = "🌐 *Tilni tanlash*\n\nJoriy til: {0} {1}\n\nInterfeys tilini tanlang:",
            ["language_changed"] = "✅ Til o'zgartirildi!\n\n{0} {1}",

            // Game cancellation
            ["game_cancelled"] = "❌ O'yin bekor qilindi.",
            ["opponent_left"] = "😔 Raqib o'yindan chiqdi.",
            ["opponent_cancelled"] = "😔 Raqib o'yinni bekor qildi.",
            ["active_game_exists"] = "⚠️ Sizda allaqachon faol o'yin bor!",
            ["game_not_found"] = "❌ O'yin topilmadi.",

            // Help
            ["help"] = "🎯 *Viktorina* — do'stlar bilan raqobatlashadigan o'yin!\n\n🎮 *Qanday o'ynash:*\n1. «O'ynash» tugmasini bosing\n2. Tez o'yin yoki do'st bilan o'ynashni tanlang\n3. Raqibdan tezroq javob bering!\n\n🏆 Eng ko'p to'g'ri javob bergan g'olib. Tenglik bo'lsa — eng tez!",

            // Category Groups
            ["category_groups"] = "📁 *Kategoriya bo'limlari:*",
            ["group_general"] = "📚 Umumiy",
            ["group_special"] = "⭐ Maxsus",
            ["group_popular"] = "🔥 Mashhur",
            ["group_my"] = "👤 Mening kategoriyalarim",
            ["group_all"] = "🎲 Istalgan kategoriya",
            ["no_categories_found"] = "😔 Bu bo'limda hali kategoriyalar yo'q.",

            // Misc
            ["player"] = "O'yinchi",
        },

        ["en"] = new Dictionary<string, string>
        {
            // Welcome & Menu
            ["welcome"] = "🎯 *Quiz Game*\n\nChoose an action:",
            ["select_country"] = "🌍 *Welcome!*\n\nPlease select your country:",
            ["play_menu"] = "🎮 *Select game mode:*",
            ["profile_menu"] = "👤 *My Profile*\n\nSelect a section:",
            ["friends_menu"] = "👥 *Friends*\n\nChoose an action:",

            // Buttons
            ["btn_play"] = "🎮 Play",
            ["btn_quick_game"] = "⚡ Quick Game",
            ["btn_play_with_friend"] = "👤 Play with Friend",
            ["btn_profile"] = "👤 My Profile",
            ["btn_statistics"] = "📊 Statistics",
            ["btn_language"] = "🌐 Language",
            ["btn_leaders"] = "🏆 Leaders",
            ["btn_friends"] = "👥 Friends",
            ["btn_my_friends"] = "📋 My Friends",
            ["btn_add_friend"] = "➕ Add Friend",
            ["btn_requests"] = "📩 Requests",
            ["btn_back"] = "🔙 Back",
            ["btn_back_to_profile"] = "🔙 To Profile",
            ["btn_cancel"] = "❌ Cancel",
            ["btn_leave_game"] = "❌ Leave Game",
            ["btn_ready"] = "✅ Ready",
            ["btn_check_opponent"] = "🔄 Check",
            ["btn_help"] = "❓ Help",
            ["btn_accept"] = "✅ Accept",
            ["btn_reject"] = "❌ Reject",
            ["btn_to_menu"] = "🏠 To Menu",

            // Game
            ["searching_opponent"] = "🔍 *Searching for opponent...*\n\nPlease wait for someone to join.",
            ["searching_category"] = "🔍 *Searching for opponent in selected category...*",
            ["opponent_found"] = "🎮 *Opponent found!*",
            ["select_category"] = "📚 *Select category:*",
            ["select_category_friend"] = "📚 *Select category to play with friend:*",
            ["any_category"] = "🎲 Any category",
            ["game_starting"] = "🚀 *Game starting!*",
            ["waiting_opponent"] = "⏳ Waiting for opponent...",
            ["waiting_ready"] = "✅ You're ready! Waiting for opponent...",
            ["question"] = "❓ *Question {0}/{1}*",
            ["question_label"] = "Question",
            ["correct"] = "✅ Correct!",
            ["incorrect"] = "❌ Incorrect!",
            ["correct_answer"] = "Correct answer: *{0}*",
            ["your_time"] = "⏱ Your time: {0} sec",
            ["time_up"] = "⏱ *Time's up!*\n\nCorrect answer: *{0}*",
            ["opponent_answering"] = "⏳ Waiting for opponent's answer...",
            ["opponent_still_answering"] = "⏳ Opponent is still answering...",

            // Results
            ["you_won"] = "🏆 You won!",
            ["you_lost"] = "😔 You lost",
            ["draw"] = "🤝 Draw!",
            ["your_result"] = "📊 *Your result:*",
            ["correct_answers"] = "✅ Correct: {0}",
            ["time_spent"] = "⏱ Time: {0} sec",
            ["opponent_result"] = "📊 *Opponent:* {0} {1}",
            ["win_by_answers"] = "by number of correct answers",
            ["win_by_time"] = "by time",

            // Statistics
            ["your_statistics"] = "📊 *Your Statistics*\n\n🎮 Games played: *{0}*\n🏆 Wins: *{1}*\n📈 Win rate: *{2}%*\n✅ Correct answers: *{3}*",

            // Leaderboard
            ["leaderboard"] = "🏆 *Leaderboard*",
            ["leaderboard_empty"] = "🏆 *Leaderboard*\n\nNo players with completed games yet.\n\nPlay your first game!",
            ["your_position"] = "📍 *Your position:* #{0}",
            ["play_to_rank"] = "📍 Play a game to get ranked!",
            ["wins"] = "wins",
            ["games"] = "games",

            // Friends
            ["no_friends"] = "😔 You don't have any friends yet.\n\nPress «Add Friend» to find players!",
            ["select_friend"] = "👤 *Select a friend to play:*",
            ["no_friends_for_game"] = "😔 You don't have any friends yet.\n\nAdd friends first in the «Friends» section!",
            ["friend_search"] = "🔍 Enter friend's @username to invite (example: @ivan or ivan):",
            ["friend_not_found"] = "❌ User not found.",
            ["already_friends"] = "👥 You're already friends!",
            ["friend_request_sent"] = "✅ Friend request sent!",
            ["request_exists"] = "⚠️ Request already exists.",
            ["game_invitation_sent"] = "✅ Invitation sent!",
            ["game_invite_from"] = "🎮 {0} is inviting you to play! Choose a category:",
            ["you_in_game"] = "⚠️ You already have an active game.",
            ["opponent_in_game"] = "⚠️ This player is already in a game. Try later.",
            ["game_invitation_sent"] = "✅ Taklif yuborildi!",
            ["game_invite_from"] = "🎮 {0} sizni o'yinga taklif qilmoqda! Kategoriya tanlang:",
            ["you_in_game"] = "⚠️ Sizda allaqachon faol o'yin bor.",
            ["opponent_in_game"] = "⚠️ Bu o'yinchi allaqachon o'yinda. Keyinroq urinib ko'ring.",
            ["game_invitation_sent"] = "✅ Einladung gesendet!",
            ["game_invite_from"] = "🎮 {0} lädt dich zum Spielen ein! Wähle eine Kategorie:",
            ["you_in_game"] = "⚠️ Du hast bereits ein aktives Spiel.",
            ["opponent_in_game"] = "⚠️ Dieser Spieler ist bereits im Spiel. Versuche es später.",
            ["game_invitation_sent"] = "✅ دعوت ارسال شد!",
            ["game_invite_from"] = "🎮 {0} شما را به بازی دعوت می‌کند! دسته را انتخاب کنید:",
            ["you_in_game"] = "⚠️ شما قبلاً یک بازی فعال دارید.",
            ["opponent_in_game"] = "⚠️ این بازیکن در حال بازی است. بعداً امتحان کنید.",
            ["game_invitation_sent"] = "✅ Convite enviado!",
            ["game_invite_from"] = "🎮 {0} está convidando você para jogar! Escolha a categoria:",
            ["you_in_game"] = "⚠️ Você já tem um jogo ativo.",
            ["opponent_in_game"] = "⚠️ Este jogador já está em jogo. Tente mais tarde.",
            ["game_invitation_sent"] = "✅ निमंत्रण भेजा गया!",
            ["game_invite_from"] = "🎮 {0} आपको खेलने के लिए आमंत्रित कर रहा है! श्रेणी चुनें:",
            ["you_in_game"] = "⚠️ आपका पहले से एक सक्रिय गेम है।",
            ["opponent_in_game"] = "⚠️ यह खिलाड़ी पहले से खेल में है। बाद में कोशिश करें।",
            ["game_invitation_sent"] = "✅ Приглашение отправлено!",
            ["game_invite_from"] = "🎮 {0} приглашает вас сыграть! Выберите категорию:",
            ["you_in_game"] = "⚠️ У вас уже есть активная игра.",
            ["opponent_in_game"] = "⚠️ Этот игрок уже в игре. Попробуйте позже.",
            ["new_friend_request"] = "📩 You have a new friend request!",
            ["no_requests"] = "📭 No incoming friend requests.",
            ["incoming_requests"] = "📩 *Incoming requests:*",
            ["friend_accepted"] = "✅ You accepted the friend request!",
            ["friend_rejected"] = "❌ Request rejected.",
            ["accept_failed"] = "❌ Failed to accept request.",

            // Game invites
            ["invite_sent"] = "📨 Invitation sent!",
            ["waiting_response"] = "Waiting for response...",
            ["click_ready"] = "Click «Ready» when your friend accepts the invitation.",
            ["game_invite"] = "🎮 *{0}* invites you to play!",
            ["category_info"] = "\n📚 Category: *{0}*",

            // Language
            ["language_selection"] = "🌐 *Language Selection*\n\nCurrent language: {0} {1}\n\nSelect interface language:",
            ["language_changed"] = "✅ Language changed!\n\n{0} {1}",

            // Game cancellation
            ["game_cancelled"] = "❌ Game cancelled.",
            ["opponent_left"] = "😔 Opponent left the game.",
            ["opponent_cancelled"] = "😔 Opponent cancelled the game.",
            ["active_game_exists"] = "⚠️ You already have an active game!",
            ["game_not_found"] = "❌ Game not found.",

            // Help
            ["help"] = "🎯 *Quiz Game* — compete with friends!\n\n🎮 *How to play:*\n1. Press «Play»\n2. Choose quick game or play with a friend\n3. Answer questions faster than your opponent!\n\n🏆 Winner is who gives more correct answers. If tied — who's faster!",

            // Category Groups
            ["category_groups"] = "📁 *Category sections:*",
            ["group_general"] = "📚 General",
            ["group_special"] = "⭐ Special",
            ["group_popular"] = "🔥 Popular",
            ["group_my"] = "👤 My Categories",
            ["group_all"] = "🎲 Any category",
            ["no_categories_found"] = "😔 No categories in this section yet.",

            // Misc
            ["player"] = "Player",
        },
    };

    public static readonly Dictionary<string, (string Flag, string Name)> Languages = new()
    {
        ["ru"] = ("🇷🇺", "Русский"),
        ["hi"] = ("🇮🇳", "हिन्दी"),
        ["pt"] = ("🇧🇷", "Português"),
        ["fa"] = ("🇮🇷", "فارسی"),
        ["de"] = ("🇩🇪", "Deutsch"),
        ["uz"] = ("🇺🇿", "O'zbek"),
        ["en"] = ("🇬🇧", "English"),
    };

    public static readonly Dictionary<string, string> CountryToLanguage = new()
    {
        ["RU"] = "ru",  // Russia
        ["IN"] = "hi",  // India
        ["BR"] = "pt",  // Brazil
        ["IR"] = "fa",  // Iran
        ["DE"] = "de",  // Germany
        ["UZ"] = "uz",  // Uzbekistan
        ["US"] = "en",  // United States
        ["GB"] = "en",  // United Kingdom
        ["CA"] = "en",  // Canada
        ["AU"] = "en",  // Australia
    };

    public static string Get(string languageCode, string key)
    {
        if (Translations.TryGetValue(languageCode, out var langDict) &&
            langDict.TryGetValue(key, out var translation))
        {
            return translation;
        }

        // Fallback to Russian (most complete)
        if (Translations["ru"].TryGetValue(key, out var russianTranslation))
        {
            return russianTranslation;
        }

        return key;
    }

    public static string Get(string languageCode, string key, params object[] args)
    {
        var template = Get(languageCode, key);
        return string.Format(template, args);
    }

    public static string GetLanguageFromCountry(string? countryCode)
    {
        if (string.IsNullOrEmpty(countryCode))
            return "ru";

        return CountryToLanguage.TryGetValue(countryCode.ToUpper(), out var lang) ? lang : "ru";
    }

    public static (string Flag, string Name) GetLanguageInfo(string languageCode)
    {
        return Languages.TryGetValue(languageCode, out var info) ? info : ("🌐", languageCode);
    }
}
