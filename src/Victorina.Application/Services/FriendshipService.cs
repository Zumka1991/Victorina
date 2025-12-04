using Microsoft.EntityFrameworkCore;
using Victorina.Application.Interfaces;
using Victorina.Domain.Entities;
using Victorina.Domain.Enums;
using Victorina.Infrastructure.Data;

namespace Victorina.Application.Services;

public class FriendshipService : IFriendshipService
{
    private readonly VictorinaDbContext _context;

    public FriendshipService(VictorinaDbContext context)
    {
        _context = context;
    }

    public async Task<Friendship?> SendFriendRequestAsync(int requesterId, int addresseeId)
    {
        if (requesterId == addresseeId) return null;

        var existing = await _context.Friendships
            .FirstOrDefaultAsync(f =>
                (f.RequesterId == requesterId && f.AddresseeId == addresseeId) ||
                (f.RequesterId == addresseeId && f.AddresseeId == requesterId));

        if (existing != null) return null;

        var friendship = new Friendship
        {
            RequesterId = requesterId,
            AddresseeId = addresseeId,
            Status = FriendshipStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.Friendships.Add(friendship);
        await _context.SaveChangesAsync();

        return friendship;
    }

    public async Task<bool> AcceptFriendRequestAsync(int friendshipId, int userId)
    {
        var friendship = await _context.Friendships.FindAsync(friendshipId);

        if (friendship == null || friendship.AddresseeId != userId || friendship.Status != FriendshipStatus.Pending)
            return false;

        friendship.Status = FriendshipStatus.Accepted;
        friendship.AcceptedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RejectFriendRequestAsync(int friendshipId, int userId)
    {
        var friendship = await _context.Friendships.FindAsync(friendshipId);

        if (friendship == null || friendship.AddresseeId != userId || friendship.Status != FriendshipStatus.Pending)
            return false;

        friendship.Status = FriendshipStatus.Rejected;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemoveFriendAsync(int userId, int friendId)
    {
        var friendship = await _context.Friendships
            .FirstOrDefaultAsync(f =>
                f.Status == FriendshipStatus.Accepted &&
                ((f.RequesterId == userId && f.AddresseeId == friendId) ||
                 (f.RequesterId == friendId && f.AddresseeId == userId)));

        if (friendship == null) return false;

        _context.Friendships.Remove(friendship);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IList<User>> GetFriendsAsync(int userId)
    {
        var friendships = await _context.Friendships
            .Include(f => f.Requester)
            .Include(f => f.Addressee)
            .Where(f => f.Status == FriendshipStatus.Accepted &&
                       (f.RequesterId == userId || f.AddresseeId == userId))
            .ToListAsync();

        return friendships
            .Select(f => f.RequesterId == userId ? f.Addressee : f.Requester)
            .ToList();
    }

    public async Task<IList<Friendship>> GetPendingRequestsAsync(int userId)
    {
        return await _context.Friendships
            .Include(f => f.Requester)
            .Where(f => f.AddresseeId == userId && f.Status == FriendshipStatus.Pending)
            .ToListAsync();
    }

    public async Task<bool> AreFriendsAsync(int userId1, int userId2)
    {
        return await _context.Friendships
            .AnyAsync(f =>
                f.Status == FriendshipStatus.Accepted &&
                ((f.RequesterId == userId1 && f.AddresseeId == userId2) ||
                 (f.RequesterId == userId2 && f.AddresseeId == userId1)));
    }
}
