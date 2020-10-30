using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace AuthServer.Server.Models
{
    public class AuthDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        public DbSet<AuthSession> AuthSessions { get; set; }
    }

    public class AuthSession
    {
        public int Id { get; set; }
        public Instant CreationTime { get; set; }
    }

    public class AppUser : IdentityUser<Guid>
    {
        
    }
}
