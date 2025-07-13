using JaeZooServer.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
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
    [HttpGet]
    public async Task<IActionResult> Ping()
    {
        var sw = Stopwatch.StartNew();

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var user = await _db.Users.FindAsync(userId);
        if (user == null)
            return NotFound();

        user.LastPing = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        sw.Stop();
        return Ok(new { ping = sw.ElapsedMilliseconds });
    }
}
