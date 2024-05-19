using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MicrobotApi.Database;

public class MicrobotContext : DbContext
{
    public MicrobotContext(DbContextOptions options)
        : base(options)
    {
    }
    
    public DbSet<Session> Sessions { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<DiscordUser> DiscordUsers { get; set; }
    public DbSet<ScriptKey> Keys { get; set; }

}

[Index(nameof(DiscordId))]
public class DiscordUser : User
{
    [MaxLength(255)]
    public string DiscordId { get; set; }

    public List<ScriptKey> Keys { get; set; }
}

public class User
{
    public Guid Id { get; set; }
    public string Token { get; set; }
    public string RefreshToken { get; set; }
    public DateTime TokenExpiry { get; set; }
    public bool IsPremium { get; set; }
    public string Type { get; set; }

}

public class Session
{
    public Guid Id { get; set; }
    public bool IsLoggedIn { get; set; }

    public string Version { get; set; }
    public DateTime LastPing { get; set; }
}

public class ScriptKey
{
    public Guid Id { get; set; }
    public string Key { get; set; }
    public string HWID { get; set; }
    public bool Active { get; set; }
    public string PaymentReference { get; set; }
}