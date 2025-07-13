using JaeZooServer.Data;
using JaeZooServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using JaeZooServer.DTOs;

namespace JaeZooServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _db;

    public UserController(AppDbContext db)
    {
        _db = db;
    }

    [Authorize]
    [HttpGet("search")]
    public async Task<IActionResult> Search(string query)
    {
        var users = await _db.Users
            .Where(u => u.Username.Contains(query))
            .Select(u => new { u.Id, u.Username })
            .ToListAsync();

        return Ok(users);
    }

    [Authorize]
    [HttpPost("add-friend/{receiverId}")]
    public async Task<IActionResult> AddFriend(int receiverId)
    {
        var senderId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (senderId == receiverId)
            return BadRequest("Нельзя добавить себя в друзья");

        var existing = await _db.FriendRequests
            .FirstOrDefaultAsync(fr =>
                fr.SenderId == senderId && fr.ReceiverId == receiverId);

        if (existing != null)
            return BadRequest("Запрос уже отправлен");

        var request = new FriendRequest
        {
            SenderId = senderId,
            ReceiverId = receiverId
        };

        _db.FriendRequests.Add(request);
        await _db.SaveChangesAsync();

        return Ok("Запрос отправлен");
    }

    [Authorize]
    [HttpGet("friends")]
    public async Task<IActionResult> GetFriends()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var utcNow = DateTime.UtcNow;

        var friends = await _db.FriendRequests
            .Where(fr => (fr.SenderId == userId || fr.ReceiverId == userId) && fr.IsAccepted)
            .Select(fr => new UserDto
            {
                Id = fr.SenderId == userId ? fr.Receiver.Id : fr.Sender.Id,
                Username = fr.SenderId == userId ? fr.Receiver.Username : fr.Sender.Username,
                IsOnline = (fr.SenderId == userId ? fr.Receiver.LastPing : fr.Sender.LastPing) > utcNow.AddMinutes(-1)
            })
            .ToListAsync();

        return Ok(friends);
    }

    [Authorize]
    [HttpPost("accept-friend/{requestId}")]
    public async Task<IActionResult> AcceptFriend(int requestId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var request = await _db.FriendRequests
            .Include(fr => fr.Receiver)
            .FirstOrDefaultAsync(fr => fr.Id == requestId && fr.ReceiverId == userId);

        if (request == null)
            return NotFound("Запрос не найден");

        request.IsAccepted = true;
        await _db.SaveChangesAsync();

        return Ok("Друг добавлен");
    }

}
