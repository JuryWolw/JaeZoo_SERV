namespace JaeZooServer.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastPing { get; set; } = DateTime.UtcNow;
    public List<FriendRequest> SentRequests { get; set; } = new();
    public List<FriendRequest> ReceivedRequests { get; set; } = new();
}