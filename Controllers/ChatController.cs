using JaeZooServer.Data;
using JaeZooServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JaeZooServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly AppDbContext _db;

    public ChatController(AppDbContext db)
    {
        _db = db;
    }

    [Authorize]
    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] MessageDto dto)
    {
        var senderId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var receiver = await _db.Users.FindAsync(dto.ReceiverId);
        if (receiver == null) return NotFound("Пользователь не найден");

        var msg = new Message
        {
            SenderId = senderId,
            ReceiverId = dto.ReceiverId,
            Content = dto.Content
        };

        _db.Messages.Add(msg);
        await _db.SaveChangesAsync();

        return Ok(new { msg.Id, msg.Content, msg.SentAt });
    }

    [Authorize]
    [HttpGet("with/{userId}")]
    public async Task<IActionResult> GetMessages(int userId)
    {
        var myId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var messages = await _db.Messages
            .Where(m =>
                (m.SenderId == myId && m.ReceiverId == userId) ||
                (m.SenderId == userId && m.ReceiverId == myId))
            .OrderBy(m => m.SentAt)
            .Select(m => new
            {
                m.Id,
                m.Content,
                m.SentAt,
                From = m.SenderId == myId ? "me" : "them"
            })
            .ToListAsync();

        return Ok(messages);
    }
}

public class MessageDto
{
    public int ReceiverId { get; set; }
    public string Content { get; set; } = string.Empty;
}
