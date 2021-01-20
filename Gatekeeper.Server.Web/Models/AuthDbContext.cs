using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuthSessionIp>()
                .HasIndex(b => b.IpAddress);

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<AuthSession> AuthSessions { get; set; } = null!;
        public DbSet<AuthApp> AuthApp { get; set; } = null!;
        public DbSet<LdapAppSettings> LdapAppSettings { get; set; } = null!;
        public DbSet<LdapAppUserCredentials> LdapAppUserCredentials { get; set; } = null!;
        public DbSet<UserGroup> UserGroup { get; set; } = null!;
        public DbSet<SystemSetting> SystemSettings { get; set; } = null!;
        public DbSet<UserTotpDevice> UserTotpDevices { get; set; } = null!;
        public DbSet<InvalidLoginAttempt> InvalidLoginAttempts { get; set; } = null!;
        public DbSet<InvalidTwoFactorAttempt> InvalidTwoFactorAttempts { get; set; } = null!;
        public DbSet<OIDCSession> OIDCSessions { get; set; } = null!;
        public DbSet<OIDCAppSettings> OIDCAppSettings { get; set; } = null!;
        public DbSet<ProxyAppSettings> ProxyAppSettings { get; set; } = null!;
        public DbSet<SCIMAppSettings> SCIMAppSettings { get; set; } = null!;
        public DbSet<ScimUserSyncState> ScimUserSyncStates { get; set; } = null!;
        public DbSet<ScimGroupSyncState> ScimGroupSyncStates { get; set; } = null!;
        public DbSet<AuthSessionIp> AuthSessionIps { get; set; } = null!;
        public DbSet<SystemSecurityAlert> SystemSecurityAlerts { get; set; } = null!;
        public DbSet<UserSecurityAlert> UserSecurityAlerts { get; set; } = null!;
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
        public HostingTypeEnum HostingType { get; set; }
        public DirectoryMethodEnum DirectoryMethod { get; set; }
        public AuthMethodEnum AuthMethod { get; set; }
        public LdapAppSettings? LdapAppSettings { get; set; }
        public OIDCAppSettings? OidcAppSettings { get; set; }
        public ProxyAppSettings? ProxyAppSettings { get; set; }
        public SCIMAppSettings? ScimAppSettings { get; set; }
        public ICollection<UserGroup> UserGroups { get; set; } = null!;
        public string? Description { get; set; }
        public string? Url { get; set; }

        public enum HostingTypeEnum
        {
            WEB_GENERIC = 1,
            WEB_GATEKEEPER_PROXY = 3,
            NON_WEB = 4,
        }

        public enum DirectoryMethodEnum
        {
            NONE = 0,
            LDAP = 1,
            SCIM = 2,
        }

        public enum AuthMethodEnum
        {
            NONE = 0,
            LDAP = 1,
            OIDC = 2,
        }
    }

    public class SystemSecurityAlert : SecurityAlert
    { }

    public class UserSecurityAlert : SecurityAlert
    {
        public AppUser Recipient { get; set; } = null!;
    }

    public class SecurityAlert
    {
        public Guid Id { get; set; }
        public Gatekeeper.Server.Web.Services.Alerts.Types.AlertTypeEnum AlertType { get; set; }
        public Dictionary<string, string> KeyValueStore { get; set; } = null!;
    }

    public class SCIMAppSettings
    {
        public Guid Id { get; set; }
        public Guid AuthAppId { get; set; }
        public AuthApp AuthApp { get; set; } = null!;
        public string Endpoint { get; set; } = null!;
        public string Credentials { get; set; } = null!;
        public ICollection<ScimUserSyncState> ScimUserSyncStates { get; set; } = null!;
        public ICollection<ScimGroupSyncState> ScimGroupSyncStates { get; set; } = null!;
    }

    public class ScimUserSyncState
    {
        public Guid Id { get; set; }
        public Guid SCIMAppSettingsId { get; set; }
        public SCIMAppSettings SCIMAppSettings { get; set; } = null!;
        public AppUser User { get; set; } = null!;
        public string ServiceId { get; set; } = null!;
    }

    public class ScimGroupSyncState
    {
        public Guid Id { get; set; }
        public Guid SCIMAppSettingsId { get; set; }
        public SCIMAppSettings SCIMAppSettings { get; set; } = null!;
        public UserGroup UserGroup { get; set; } = null!;
        public string ServiceId { get; set; } = null!;
    }

    public class ProxyAppSettings
    {
        public Guid Id { get; set; }
        public Guid AuthAppId { get; set; }
        public AuthApp AuthApp { get; set; } = null!;
        public string InternalHostname { get; set; } = null!;
        public string PublicHostname { get; set; } = null!;
        public List<string>? EndpointsWithoutAuth { get; set; }
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
        public IEnumerable<LdapAppUserCredentials> LdapAppUserCredentials { get; set; } = null!;
    }

    public class LdapAppUserCredentials
    {
        public Guid Id { get; set; }
        public LdapAppSettings LdapAppSettings { get; set; } = null!;
        public AppUser User { get; set; } = null!;
        public string HashedPassword { get; set; } = null!;
    }

    public class OIDCAppSettings
    {
        public Guid Id { get; set; }
        public Guid AuthAppId { get; set; }
        public AuthApp AuthApp { get; set; } = null!;
        public ICollection<OIDCSession> OIDCSessions { get; set; } = null!;
        public string ClientId { get; set; } = null!;
        public string ClientSecret { get; set; } = null!;
        public string Audience { get; set; } = null!;
        public string RedirectUrl { get; set; } = null!;
    }

    public class OIDCSession
    {
        public Guid Id { get; set; }
        public OIDCAppSettings OIDCAppSettings { get; set; } = null!;
        public AppUser User { get; set; } = null!;
        public string Nonce { get; set; } = null!;
        public Instant CreationTime { get; set; }
        public Instant? ExpiredTime { get; set; }
    }

    public class AuthSession
    {
        public Guid Id { get; set; }
        public Instant CreationTime { get; set; }
        public AppUser User { get; set; } = null!;
        public Instant LastUsedTime { get; set; }
        public Instant? ExpiredTime { get; set; }
        public ICollection<AuthSessionIp> SessionIps { get; set; } = null!;
        public string? UserAgent { get; set; }
        [Column(TypeName = "jsonb")]
        public DeviceInformation? DeviceInfo { get; set; }
    }

    public class DeviceInformation
    {
        public DeviceTypeEnum DeviceType { get; set; }
        public string? OperatingSystem { get; set; }
        public string? Browser { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }

        public enum DeviceTypeEnum
        {
            Unknown = 0,
            Smartphone = 1,
            Tablet = 2,
            Desktop = 3,
        }
    }

    public class AuthSessionIp
    {
        public Guid Id { get; set; }
        public IPAddress IpAddress { get; set; } = null!;
        public AuthSession AuthSession { get; set; } = null!;
        public string? City { get; set; }
        public string? Country { get; set; }
    }

    public class UserGroup
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public ICollection<AppUser> Members { get; set; } = null!;
        public ICollection<AuthApp> AuthApps { get; set; } = null!;
        public ICollection<ScimGroupSyncState> ScimGroupSyncState { get; set; } = null!;
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
        public ICollection<InvalidLoginAttempt> InvalidLoginAttempts { get; set; } = null!;
        public ICollection<InvalidTwoFactorAttempt> InvalidTwoFactorAttempts { get; set; } = null!;
        public ICollection<ScimUserSyncState> ScimUserSyncState { get; set; } = null!;
    }

    public class InvalidLoginAttempt
    {
        public Guid Id { get; set; }
        public AppUser? TargetUser { get; set; }
        public string UserName { get; set; } = null!;
        public Instant AttemptTime { get; set; }
        public IPAddress IpAddress { get; set; } = null!;
        public string UserAgent { get; set; } = null!;
    }

    public class InvalidTwoFactorAttempt
    {
        public Guid Id { get; set; }
        public AppUser TargetUser { get; set; } = null!;
        public Instant AttemptTime { get; set; }
        public IPAddress IPAddress { get; set; } = null!;
        public string UserAgent { get; set; } = null!;
    }
}
