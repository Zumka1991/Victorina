using Telegram.Bot.Types.ReplyMarkups;
using Victorina.Bot.Constants;
using Victorina.Domain.Entities;

namespace Victorina.Bot.Services;

public class KeyboardService
{
    // ============ REPLY KEYBOARDS (постоянные кнопки внизу) ============

    public ReplyKeyboardMarkup GetMainMenuReplyKeyboard()
    {
        return new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "🎮 Играть" },
            new KeyboardButton[] { "👤 Мой профиль", "🏆 Лидеры" },
            new KeyboardButton[] { "❓ Помощь" }
        })
        {
            ResizeKeyboard = true,
            IsPersistent = true
        };
    }

    public ReplyKeyboardMarkup GetProfileMenuReplyKeyboard()
    {
        return new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "📊 Статистика", "🌍 Страна" },
            new KeyboardButton[] { "👥 Друзья" },
            new KeyboardButton[] { "🔙 Назад" }
        })
        {
            ResizeKeyboard = true
        };
    }

    public ReplyKeyboardMarkup GetPlayMenuReplyKeyboard()
    {
        return new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "⚡ Быстрая игра" },
            new KeyboardButton[] { "👤 Играть с другом" },
            new KeyboardButton[] { "🔙 Назад" }
        })
        {
            ResizeKeyboard = true
        };
    }

    public ReplyKeyboardMarkup GetFriendsMenuReplyKeyboard()
    {
        return new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "📋 Мои друзья" },
            new KeyboardButton[] { "➕ Добавить друга", "📩 Запросы" },
            new KeyboardButton[] { "🔙 В профиль" }
        })
        {
            ResizeKeyboard = true
        };
    }

    public ReplyKeyboardMarkup GetCancelReplyKeyboard()
    {
        return new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "❌ Отмена" }
        })
        {
            ResizeKeyboard = true
        };
    }

    public ReplyKeyboardMarkup GetGameReplyKeyboard()
    {
        return new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "❌ Покинуть игру" }
        })
        {
            ResizeKeyboard = true
        };
    }

    // ============ INLINE KEYBOARDS (кнопки в сообщениях) ============

    public InlineKeyboardMarkup GetPlayInlineKeyboard()
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("⚡ Быстрая игра", CallbackData.QuickGame) },
            new[] { InlineKeyboardButton.WithCallbackData("👤 Играть с другом", CallbackData.PlayWithFriend) }
        });
    }

    public InlineKeyboardMarkup GetSearchingKeyboard()
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("🔄 Проверить", CallbackData.CheckGame) },
            new[] { InlineKeyboardButton.WithCallbackData("❌ Отмена", CallbackData.CancelGame) }
        });
    }

    public InlineKeyboardMarkup GetReadyKeyboard()
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("✅ Готов!", CallbackData.Ready) },
            new[] { InlineKeyboardButton.WithCallbackData("❌ Отмена", CallbackData.CancelGame) }
        });
    }

    public InlineKeyboardMarkup GetQuestionKeyboard(string[] answers)
    {
        var buttons = new List<InlineKeyboardButton[]>();
        var emojis = new[] { "1️⃣", "2️⃣", "3️⃣", "4️⃣" };

        for (int i = 0; i < answers.Length && i < 4; i++)
        {
            buttons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    $"{emojis[i]} {answers[i]}",
                    $"{CallbackData.Answer}{i}")
            });
        }

        return new InlineKeyboardMarkup(buttons);
    }

    public InlineKeyboardMarkup GetWaitingOpponentKeyboard()
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("🔄 Проверить", CallbackData.CheckOpponent) }
        });
    }

    public InlineKeyboardMarkup GetFriendsListKeyboard(IList<User> friends)
    {
        var buttons = new List<InlineKeyboardButton[]>();

        foreach (var friend in friends)
        {
            var displayName = !string.IsNullOrEmpty(friend.Username)
                ? $"@{friend.Username}"
                : friend.FirstName ?? "Друг";

            buttons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    $"🎮 {displayName}",
                    $"{CallbackData.InviteFriend}{friend.Id}")
            });
        }

        buttons.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData("🔙 Назад", CallbackData.Friends)
        });

        return new InlineKeyboardMarkup(buttons);
    }

    public InlineKeyboardMarkup GetFriendRequestsKeyboard(IList<Friendship> requests)
    {
        var buttons = new List<InlineKeyboardButton[]>();

        foreach (var req in requests)
        {
            var displayName = !string.IsNullOrEmpty(req.Requester.Username)
                ? $"@{req.Requester.Username}"
                : req.Requester.FirstName ?? "Игрок";

            buttons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData($"👤 {displayName}", "_")
            });
            buttons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData("✅ Принять", $"{CallbackData.AcceptFriend}{req.Id}"),
                InlineKeyboardButton.WithCallbackData("❌ Отклонить", $"{CallbackData.RejectFriend}{req.Id}")
            });
        }

        buttons.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData("🔙 Назад", CallbackData.Friends)
        });

        return new InlineKeyboardMarkup(buttons);
    }

    public InlineKeyboardMarkup GetBackToMenuKeyboard()
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("🏠 В меню", CallbackData.BackToMenu) }
        });
    }

    public InlineKeyboardMarkup GetCountrySelectionKeyboard()
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🇷🇺 Россия", $"{CallbackData.SelectCountry}RU"),
                InlineKeyboardButton.WithCallbackData("🇺🇦 Украина", $"{CallbackData.SelectCountry}UA")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🇧🇾 Беларусь", $"{CallbackData.SelectCountry}BY"),
                InlineKeyboardButton.WithCallbackData("🇰🇿 Казахстан", $"{CallbackData.SelectCountry}KZ")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🇺🇿 Узбекистан", $"{CallbackData.SelectCountry}UZ"),
                InlineKeyboardButton.WithCallbackData("🇦🇿 Азербайджан", $"{CallbackData.SelectCountry}AZ")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🇬🇪 Грузия", $"{CallbackData.SelectCountry}GE"),
                InlineKeyboardButton.WithCallbackData("🇦🇲 Армения", $"{CallbackData.SelectCountry}AM")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🇲🇩 Молдова", $"{CallbackData.SelectCountry}MD"),
                InlineKeyboardButton.WithCallbackData("🇰🇬 Кыргызстан", $"{CallbackData.SelectCountry}KG")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🇩🇪 Германия", $"{CallbackData.SelectCountry}DE"),
                InlineKeyboardButton.WithCallbackData("🇵🇱 Польша", $"{CallbackData.SelectCountry}PL")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🇺🇸 США", $"{CallbackData.SelectCountry}US"),
                InlineKeyboardButton.WithCallbackData("🇬🇧 Великобритания", $"{CallbackData.SelectCountry}GB")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🇮🇱 Израиль", $"{CallbackData.SelectCountry}IL"),
                InlineKeyboardButton.WithCallbackData("🌍 Другая", $"{CallbackData.SelectCountry}OTHER")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🔙 Назад", CallbackData.BackToProfile)
            }
        });
    }

    public InlineKeyboardMarkup GetCategorySelectionKeyboard(IList<Category> categories, bool forFriend = false, int? friendId = null)
    {
        var prefix = forFriend ? $"{CallbackData.SelectCategoryForFriend}{friendId}_" : CallbackData.SelectCategory;
        var buttons = new List<InlineKeyboardButton[]>();

        // Кнопка "Любая категория"
        buttons.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData("🎲 Любая категория", $"{prefix}0")
        });

        // Категории по 2 в ряд
        for (int i = 0; i < categories.Count; i += 2)
        {
            var row = new List<InlineKeyboardButton>();
            var cat1 = categories[i];
            row.Add(InlineKeyboardButton.WithCallbackData(
                $"{cat1.Emoji ?? "📚"} {cat1.Name}",
                $"{prefix}{cat1.Id}"));

            if (i + 1 < categories.Count)
            {
                var cat2 = categories[i + 1];
                row.Add(InlineKeyboardButton.WithCallbackData(
                    $"{cat2.Emoji ?? "📚"} {cat2.Name}",
                    $"{prefix}{cat2.Id}"));
            }

            buttons.Add(row.ToArray());
        }

        buttons.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData("🔙 Назад", CallbackData.BackToMenu)
        });

        return new InlineKeyboardMarkup(buttons);
    }
}
