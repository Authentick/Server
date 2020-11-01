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
        public ICollection<AuthSessionUsages> Usages { get; set; } = null!;
        public Instant? ExpiredTime { get; set; }
    }

    public class AuthSessionUsages
    {
        public Guid Id { get; set; }
        public AuthSession Session { get; set; } = null!;
        public Instant LastActive { get; set; }
        public IPAddress IpAddress { get; set; } = null!;
    }

    public class AppUser : IdentityUser<Guid>
    {
        public ICollection<AuthSession> Sessions { get; set; } = null!;
    }
}
