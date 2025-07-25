﻿using JaeZooServer.Data;
using JaeZooServer.DTOs;
using JaeZooServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JaeZooServer.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;


public AuthService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<string?> RegisterAsync(AuthRequest request)
    {
        try
        {
            if (await _db.Users.AnyAsync(u => u.Username == request.Username))
                return null;

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return GenerateJwtToken(user);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Register error: " + ex.Message);
            return null;
        }
    }

    public async Task<string?> LoginAsync(string username, string password)
    {
        try
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return null;

            return GenerateJwtToken(user);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Login error: " + ex.Message);
            return null;
        }
    }

    private string GenerateJwtToken(User user)
    {
        var rawKey = _config["Jwt:Key"]!;
        Console.WriteLine($"[DEBUG] JWT key: {rawKey} (Length: {rawKey.Length})");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(rawKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Username)
    };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(3),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}