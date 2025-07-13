namespace JaeZooServer.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public bool IsOnline { get; set; }
}