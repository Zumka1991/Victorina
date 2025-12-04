using Victorina.Domain.Entities;

namespace Victorina.Application.Interfaces;

public interface IUserService
{
    Task<User> GetOrCreateUserAsync(long telegramId, string? username, string? firstName, string? lastName);
    Task<User?> GetByTelegramIdAsync(long telegramId);
    Task<User?> GetByIdAsync(int id);
    Task<User?> FindByUsernameAsync(string username);
    Task<User?> FindByPhoneAsync(string phone);
    Task UpdatePhoneAsync(int userId, string phone);
    Task UpdateCountryAsync(int userId, string? countryCode);
    Task UpdateLanguageAsync(int userId, string languageCode);
    Task UpdateLastActiveAsync(int userId);
    Task<IList<User>> SearchUsersAsync(string query, int excludeUserId);
}
