using JaeZooServer.Data;
using JaeZooServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=jaezoo.db"));

builder.Services.AddScoped<IPasswordHasher<string>, PasswordHasher<string>>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "JaeZoo API",
        Version = "v1"
    });
});

var app = builder.Build();

// DB Init
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "JaeZoo API v1");
    c.RoutePrefix = "swagger";
});

app.Urls.Add("http://0.0.0.0:80");

// =====================
// �����������
// =====================

// �����������
app.MapPost("/api/auth/register", async (AppDbContext db, IPasswordHasher<string> hasher, RegisterRequest req) =>
{
    if (await db.Users.AnyAsync(u =>
        u.Login == req.Login ||
        u.Email == req.Email ||
        u.Nickname == req.Nickname))
    {
        return Results.BadRequest("������������ � ����� �������, email ��� ��������� ��� ����������.");
    }

    var user = new User
    {
        Login = req.Login,
        Email = req.Email,
        Nickname = req.Nickname,
        PasswordHash = hasher.HashPassword(req.Login, req.Password),
        Status = "offline",
        AvatarUrl = "",
        Bio = ""
    };

    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Ok("������������ ���������������");
});

// ����
app.MapPost("/api/auth/login", async (AppDbContext db, IPasswordHasher<string> hasher, LoginRequest req) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u =>
        u.Login == req.Login ||
        u.Email == req.Login ||
        u.Nickname == req.Login);

    if (user == null)
        return Results.BadRequest("������������ �� ������");

    var result = hasher.VerifyHashedPassword(req.Login, user.PasswordHash, req.Password);
    return result == PasswordVerificationResult.Success
        ? Results.Ok("���� �������")
        : Results.BadRequest("�������� ������");
});

// =====================
// ������� ������������
// =====================

// �������� ������� �� ID
app.MapGet("/api/profile/bylogin/{login}", async (AppDbContext db, string login) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u =>
        u.Login == login || u.Email == login || u.Nickname == login);

    if (user is null)
        return Results.NotFound("������������ �� ������");

    return Results.Ok(new
    {
        user.Id,
        user.Login,
        user.Email,
        user.Nickname
    });
});

// �������� ������� �� ID
app.MapPut("/api/profile/{id:int}", async (AppDbContext db, int id, UpdateProfileDto dto) =>
{
    var user = await db.Users.FindAsync(id);
    if (user == null)
        return Results.NotFound("������������ �� ������");

    user.Nickname = dto.Nickname ?? user.Nickname;
    user.Status = dto.Status ?? user.Status;
    user.AvatarUrl = dto.AvatarUrl ?? user.AvatarUrl;
    user.Bio = dto.Bio ?? user.Bio;

    await db.SaveChangesAsync();

    return Results.Ok("������� �������");
});

app.Run();

// =====================
// DTO
// =====================
record RegisterRequest(string Login, string Email, string Nickname, string Password);
record LoginRequest(string Login, string Password);

record UserProfileDto(
    int Id,
    string Login,
    string Email,
    string Nickname,
    string Status,
    string AvatarUrl,
    string Bio
);

record UpdateProfileDto(
    string? Nickname,
    string? Status,
    string? AvatarUrl,
    string? Bio
);
