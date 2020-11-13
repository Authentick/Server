using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace AuthServer.Server.Models
{
    public class AuthDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        public DbSet<AuthSession> AuthSessions { get; set; } = null!;
    }

    public class AuthSession
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public Instant CreationTime { get; set; }
        public AppUser User { get; set; } = null!;
        public Instant LastUsedTime { get; set; }
        public Instant? ExpiredTime { get; set; }
    }

    public class AppUser : IdentityUser<Guid>
    {
        public ICollection<AuthSession> Sessions { get; set; } = null!;
    }
}
