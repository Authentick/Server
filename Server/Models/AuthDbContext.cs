using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public DbSet<AuthApp> AuthApp { get; set; } = null!;
        public DbSet<LdapAppSettings> LdapAppSettings { get; set; } = null!;
        public DbSet<LdapAppUserCredentials> LdapAppUserCredentials { get; set; } = null!;
        public DbSet<UserGroup> UserGroup { get; set; } = null!;
        public DbSet<SystemSetting> SystemSettings { get; set; } = null!;
        public DbSet<UserTotpDevice> UserTotpDevices { get; set; } = null!;
    }

    public class SystemSetting
    {
        [Key]
        public string Name { get; set; } = null!;
        public string Value { get; set; } = null!;
    }

    public class AuthApp
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public LdapAppSettings? LdapAppSettings { get; set; }
        public ICollection<UserGroup> UserGroups { get; set; } = null!;
    }

    public class LdapAppSettings
    {
        public Guid Id { get; set; }
        public Guid AuthAppId { get; set; }
        public AuthApp AuthApp { get; set; } = null!;
        public string BindUser { get; set; } = null!;
        public string BindUserPassword { get; set; } = null!;
        public string BaseDn { get; set; } = null!;
        public bool UseForAuthentication { get; set; }
        public bool UseForIdentity { get; set; }
    }

    public class LdapAppUserCredentials
    {
        public Guid Id { get; set; }
        public LdapAppSettings LdapAppSettings { get; set; } = null!;
        public AppUser User { get; set; } = null!;
        public string HashedPassword { get; set; } = null!;
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

    public class UserGroup
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public ICollection<AppUser> Members { get; set; } = null!;
        public ICollection<AuthApp> AuthApps { get; set; } = null!;
    }

    public class UserTotpDevice
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string SharedSecret { get; set; } = null!;
        public Instant CreationTime { get; set; }
        public Instant LastUsedTime { get; set; }
        public AppUser User { get; set; } = null!; 
    }

    public class AppUser : IdentityUser<Guid>
    {
        public ICollection<AuthSession> Sessions { get; set; } = null!;
        public ICollection<UserGroup> Groups { get; set; } = null!;
        public ICollection<UserTotpDevice> TotpDevices { get; set; } = null!;
    }
}
