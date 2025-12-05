using Telegram.Bot.Types.ReplyMarkups;
using Victorina.Bot.Constants;
using Victorina.Domain.Entities;

namespace Victorina.Bot.Services;

public class KeyboardService
{
    // ============ REPLY KEYBOARDS (постоянные кнопки внизу) ============

    public ReplyKeyboardMarkup GetMainMenuReplyKeyboard(string lang)
    {
        return new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { LocalizationService.Get(lang, "btn_play") },
            new KeyboardButton[] { LocalizationService.Get(lang, "btn_profile"), LocalizationService.Get(lang, "btn_leaders") },
            new KeyboardButton[] { LocalizationService.Get(lang, "btn_help") }
        })
        {
            ResizeKeyboard = true,
            IsPersistent = true
        };
    }

    public ReplyKeyboardMarkup GetProfileMenuReplyKeyboard(string lang)
    {
        return new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { LocalizationService.Get(lang, "btn_statistics"), LocalizationService.Get(lang, "btn_language") },
            new KeyboardButton[] { LocalizationService.Get(lang, "btn_friends") },
            new KeyboardButton[] { LocalizationService.Get(lang, "btn_back") }
        })
        {
            ResizeKeyboard = true
        };
    }

    public ReplyKeyboardMarkup GetPlayMenuReplyKeyboard(string lang)
    {
        return new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { LocalizationService.Get(lang, "btn_quick_game") },
            new KeyboardButton[] { LocalizationService.Get(lang, "btn_play_with_friend") },
            new KeyboardButton[] { LocalizationService.Get(lang, "btn_back") }
        })
        {
            ResizeKeyboard = true
        };
    }

    public ReplyKeyboardMarkup GetFriendsMenuReplyKeyboard(string lang)
    {
        return new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { LocalizationService.Get(lang, "btn_my_friends") },
            new KeyboardButton[] { LocalizationService.Get(lang, "btn_add_friend"), LocalizationService.Get(lang, "btn_requests") },
            new KeyboardButton[] { LocalizationService.Get(lang, "btn_back_to_profile") }
        })
        {
            ResizeKeyboard = true
        };
    }

    public ReplyKeyboardMarkup GetCancelReplyKeyboard(string lang)
    {
        return new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { LocalizationService.Get(lang, "btn_cancel") }
        })
        {
            ResizeKeyboard = true
        };
    }

    public ReplyKeyboardMarkup GetGameReplyKeyboard(string lang)
    {
        return new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { LocalizationService.Get(lang, "btn_leave_game") }
        })
        {
            ResizeKeyboard = true
        };
    }

    // ============ INLINE KEYBOARDS (кнопки в сообщениях) ============

    public InlineKeyboardMarkup GetPlayInlineKeyboard(string lang)
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData(LocalizationService.Get(lang, "btn_quick_game"), CallbackData.QuickGame) },
            new[] { InlineKeyboardButton.WithCallbackData(LocalizationService.Get(lang, "btn_play_with_friend"), CallbackData.PlayWithFriend) }
        });
    }

    public InlineKeyboardMarkup GetSearchingKeyboard(string lang)
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData(LocalizationService.Get(lang, "btn_check_opponent"), CallbackData.CheckGame) },
            new[] { InlineKeyboardButton.WithCallbackData(LocalizationService.Get(lang, "btn_cancel"), CallbackData.CancelGame) }
        });
    }

    public InlineKeyboardMarkup GetReadyKeyboard(string lang)
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData(LocalizationService.Get(lang, "btn_ready"), CallbackData.Ready) },
            new[] { InlineKeyboardButton.WithCallbackData(LocalizationService.Get(lang, "btn_cancel"), CallbackData.CancelGame) }
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

    public InlineKeyboardMarkup GetWaitingOpponentKeyboard(string lang)
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData(LocalizationService.Get(lang, "btn_check_opponent"), CallbackData.CheckOpponent) }
        });
    }

    public InlineKeyboardMarkup GetFriendsListKeyboard(IList<User> friends, string lang)
    {
        var buttons = new List<InlineKeyboardButton[]>();

        foreach (var friend in friends)
        {
            var displayName = !string.IsNullOrEmpty(friend.Username)
                ? $"@{friend.Username}"
                : friend.FirstName ?? LocalizationService.Get(lang, "player");

            buttons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    $"🎮 {displayName}",
                    $"{CallbackData.InviteFriend}{friend.Id}")
            });
        }

        buttons.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData(LocalizationService.Get(lang, "btn_back"), CallbackData.Friends)
        });

        return new InlineKeyboardMarkup(buttons);
    }

    public InlineKeyboardMarkup GetFriendRequestsKeyboard(IList<Friendship> requests, string lang)
    {
        var buttons = new List<InlineKeyboardButton[]>();

        foreach (var req in requests)
        {
            var displayName = !string.IsNullOrEmpty(req.Requester.Username)
                ? $"@{req.Requester.Username}"
                : req.Requester.FirstName ?? LocalizationService.Get(lang, "player");

            buttons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData($"👤 {displayName}", "_")
            });
            buttons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(LocalizationService.Get(lang, "btn_accept"), $"{CallbackData.AcceptFriend}{req.Id}"),
                InlineKeyboardButton.WithCallbackData(LocalizationService.Get(lang, "btn_reject"), $"{CallbackData.RejectFriend}{req.Id}")
            });
        }

        buttons.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData(LocalizationService.Get(lang, "btn_back"), CallbackData.Friends)
        });

        return new InlineKeyboardMarkup(buttons);
    }

    public InlineKeyboardMarkup GetBackToMenuKeyboard(string lang)
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData(LocalizationService.Get(lang, "btn_to_menu"), CallbackData.BackToMenu) }
        });
    }

    public InlineKeyboardMarkup GetLanguageSelectionKeyboard(string lang)
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🇷🇺 Русский", $"{CallbackData.SelectLanguage}ru"),
                InlineKeyboardButton.WithCallbackData("🇮🇳 हिन्दी", $"{CallbackData.SelectLanguage}hi")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🇧🇷 Português", $"{CallbackData.SelectLanguage}pt"),
                InlineKeyboardButton.WithCallbackData("🇮🇷 فارسی", $"{CallbackData.SelectLanguage}fa")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🇩🇪 Deutsch", $"{CallbackData.SelectLanguage}de"),
                InlineKeyboardButton.WithCallbackData("🇺🇿 O'zbek", $"{CallbackData.SelectLanguage}uz")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(LocalizationService.Get(lang, "btn_back"), CallbackData.BackToProfile)
            }
        });
    }

    public InlineKeyboardMarkup GetCategoryGroupSelectionKeyboard(string lang, bool forFriend = false, int? friendId = null)
    {
        var prefix = forFriend ? $"{CallbackData.SelectCategoryGroupForFriend}{friendId}_" : CallbackData.SelectCategoryGroup;

        var buttons = new List<InlineKeyboardButton[]>
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(LocalizationService.Get(lang, "group_general"), $"{prefix}general")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(LocalizationService.Get(lang, "group_special"), $"{prefix}special")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(LocalizationService.Get(lang, "group_popular"), $"{prefix}popular")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(LocalizationService.Get(lang, "group_my"), $"{prefix}my")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(LocalizationService.Get(lang, "group_all"), $"{prefix}all")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(LocalizationService.Get(lang, "btn_back"), CallbackData.BackToMenu)
            }
        };

        return new InlineKeyboardMarkup(buttons);
    }

    public InlineKeyboardMarkup GetCategorySelectionKeyboard(IList<Category> categories, string lang, bool forFriend = false, int? friendId = null)
    {
        var prefix = forFriend ? $"{CallbackData.SelectCategoryForFriend}{friendId}_" : CallbackData.SelectCategory;
        var buttons = new List<InlineKeyboardButton[]>();

        // Кнопка "Любая категория"
        buttons.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData(LocalizationService.Get(lang, "any_category"), $"{prefix}0")
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
            InlineKeyboardButton.WithCallbackData(LocalizationService.Get(lang, "btn_back"), CallbackData.BackToMenu)
        });

        return new InlineKeyboardMarkup(buttons);
    }
}
