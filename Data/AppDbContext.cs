using Microsoft.EntityFrameworkCore;
using JaeZooServer.Models;

namespace JaeZooServer.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Friendship> Friendships { get; set; }
}
