using JaeZooServer.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JaeZooServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PingController : ControllerBase
{
    private readonly AppDbContext _db;

    public PingController(AppDbContext db)
    {
        _db = db;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Ping()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _db.Users.FindAsync(userId);

        if (user == null)
            return NotFound();

        user.LastPing = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { pinged = DateTime.UtcNow });
    }
}
