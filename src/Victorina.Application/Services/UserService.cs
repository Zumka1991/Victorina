using Microsoft.EntityFrameworkCore;
using Victorina.Application.Interfaces;
using Victorina.Domain.Entities;
using Victorina.Infrastructure.Data;

namespace Victorina.Application.Services;

public class UserService : IUserService
{
    private readonly VictorinaDbContext _context;

    public UserService(VictorinaDbContext context)
    {
        _context = context;
    }

    public async Task<User> GetOrCreateUserAsync(long telegramId, string? username, string? firstName, string? lastName)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.TelegramId == telegramId);

        if (user == null)
        {
            user = new User
            {
                TelegramId = telegramId,
                Username = username ?? string.Empty,
                FirstName = firstName,
                LastName = lastName,
                CreatedAt = DateTime.UtcNow,
                LastActiveAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
        else
        {
            user.Username = username ?? user.Username;
            user.FirstName = firstName ?? user.FirstName;
            user.LastName = lastName ?? user.LastName;
            user.LastActiveAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return user;
    }

    public async Task<User?> GetByTelegramIdAsync(long telegramId)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.TelegramId == telegramId);
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> FindByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
    }

    public async Task<User?> FindByPhoneAsync(string phone)
    {
        var normalizedPhone = NormalizePhone(phone);
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Phone != null && u.Phone == normalizedPhone);
    }

    public async Task UpdatePhoneAsync(int userId, string phone)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.Phone = NormalizePhone(phone);
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateCountryAsync(int userId, string? countryCode)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.CountryCode = countryCode;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateLanguageAsync(int userId, string languageCode)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.LanguageCode = languageCode;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateLastActiveAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.LastActiveAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IList<User>> SearchUsersAsync(string query, int excludeUserId)
    {
        query = query.Trim().ToLower();

        return await _context.Users
            .Where(u => u.Id != excludeUserId &&
                       (u.Username.ToLower().Contains(query) ||
                        (u.FirstName != null && u.FirstName.ToLower().Contains(query)) ||
                        (u.Phone != null && u.Phone.Contains(query))))
            .Take(10)
            .ToListAsync();
    }

    private static string NormalizePhone(string phone)
    {
        return new string(phone.Where(char.IsDigit).ToArray());
    }
}
