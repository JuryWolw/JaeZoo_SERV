using JaeZooServer.DTOs;
using JaeZooServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace JaeZooServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;

    public AuthController(AuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AuthRequest request)
    {
        var token = await _auth.RegisterAsync(request);
        return token == null ? BadRequest("Пользователь уже существует") : Ok(new { token });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthRequest request)
    {
        var token = await _auth.LoginAsync(request.Username, request.Password);
        return token == null ? Unauthorized("Неверный логин или пароль") : Ok(new { token });
    }
}
