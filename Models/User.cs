namespace JaeZooServer.Models;

public class User
{
    public int Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Status { get; set; } = "offline";
    public string AvatarUrl { get; set; } = ""; // можно null, но пока пусть будет пустая строка
    public string Bio { get; set; } = "";
}
