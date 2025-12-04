using Victorina.Domain.Enums;

namespace Victorina.Domain.Entities;

public class Friendship
{
    public int Id { get; set; }
    public int RequesterId { get; set; }
    public int AddresseeId { get; set; }
    public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcceptedAt { get; set; }

    public User Requester { get; set; } = null!;
    public User Addressee { get; set; } = null!;
}
