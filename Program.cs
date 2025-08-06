using JaeZooServer.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=jaezoo.db"));

builder.Services.AddScoped<IPasswordHasher<string>, PasswordHasher<string>>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// DB Init
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.MapPost("/api/auth/register", async (AppDbContext db, IPasswordHasher<string> hasher, RegisterRequest req) =>
{
    if (await db.Users.AnyAsync(u => u.Login == req.Login))
        return Results.BadRequest("Login already exists");

    var user = new JaeZooServer.Models.User
    {
        Login = req.Login,
        Email = req.Email,
        PasswordHash = hasher.HashPassword(req.Login, req.Password)
    };

    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Ok("User registered");
});

app.MapPost("/api/auth/login", async (AppDbContext db, IPasswordHasher<string> hasher, LoginRequest req) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Login == req.Login);
    if (user == null)
        return Results.BadRequest("Invalid login");

    var result = hasher.VerifyHashedPassword(req.Login, user.PasswordHash, req.Password);
    return result == PasswordVerificationResult.Success
        ? Results.Ok("Login successful")
        : Results.BadRequest("Invalid password");
});

app.Run();

record RegisterRequest(string Login, string Email, string Password);
record LoginRequest(string Login, string Password);
