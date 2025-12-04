using System.Collections.Concurrent;

namespace Victorina.Bot.Services;

public enum UserState
{
    None,
    WaitingForFriendSearch,
    InGame
}

public class UserStateService
{
    private readonly ConcurrentDictionary<long, UserState> _states = new();
    private readonly ConcurrentDictionary<long, object?> _stateData = new();

    public UserState GetState(long telegramId)
    {
        return _states.GetValueOrDefault(telegramId, UserState.None);
    }

    public void SetState(long telegramId, UserState state, object? data = null)
    {
        _states[telegramId] = state;
        _stateData[telegramId] = data;
    }

    public T? GetStateData<T>(long telegramId) where T : class
    {
        if (_stateData.TryGetValue(telegramId, out var data))
        {
            return data as T;
        }
        return null;
    }

    public void ClearState(long telegramId)
    {
        _states.TryRemove(telegramId, out _);
        _stateData.TryRemove(telegramId, out _);
    }
}
