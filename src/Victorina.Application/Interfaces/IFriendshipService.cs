using Victorina.Domain.Entities;

namespace Victorina.Application.Interfaces;

public interface IFriendshipService
{
    Task<Friendship?> SendFriendRequestAsync(int requesterId, int addresseeId);
    Task<bool> AcceptFriendRequestAsync(int friendshipId, int userId);
    Task<bool> RejectFriendRequestAsync(int friendshipId, int userId);
    Task<bool> RemoveFriendAsync(int userId, int friendId);
    Task<IList<User>> GetFriendsAsync(int userId);
    Task<IList<Friendship>> GetPendingRequestsAsync(int userId);
    Task<bool> AreFriendsAsync(int userId1, int userId2);
}
