using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using AuthServer.Server.Services;
using AuthServer.Server.Services.Crypto;
using AuthServer.Server.Services.ReverseProxy;
using AuthServer.Server.Services.TLS;
using AuthServer.Shared.Admin;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Server.GRPC.Admin
{
    [Authorize(Policy = "SuperAdministrator")]
    public class AppsService : AuthServer.Shared.Admin.AdminApps.AdminAppsBase
    {
        private readonly AuthDbContext _authDbContext;
        private readonly IDataProtector _ldapSettingsDataProtector;
        private readonly SecureRandom _secureRandom;
        private readonly ConfigurationProvider _configurationProvider;
        private readonly MemoryPopulator _memoryPopulator;

        public AppsService(
            AuthDbContext authDbContext,
            IDataProtectionProvider dataProtectionProvider,
            SecureRandom secureRandom,
            ConfigurationProvider configurationProvider,
            MemoryPopulator memoryPopulator
            )
        {
            _authDbContext = authDbContext;
            _ldapSettingsDataProtector = dataProtectionProvider.CreateProtector("LdapSettingsDataProtector");
            _secureRandom = secureRandom;
            _configurationProvider = configurationProvider;
            _memoryPopulator = memoryPopulator;
        }

        public override async Task<AddGroupToAppReply> AddGroupToApp(AddGroupToAppRequest request, ServerCallContext context)
        {
            Guid groupId = new Guid(request.GroupId);
            UserGroup group = await _authDbContext.UserGroup
                .SingleAsync(g => g.Id == groupId);

            Guid appId = new Guid(request.AppId);
            AuthApp app = await _authDbContext.AuthApp
                .Include(a => a.UserGroups)
                .SingleAsync(a => a.Id == appId);

            app.UserGroups.Add(group);
            await _authDbContext.SaveChangesAsync();

            return new AddGroupToAppReply { Success = true };
        }

        public override async Task<AddNewAppReply> AddNewApp(AddNewAppRequest request, ServerCallContext context)
        {
            AuthApp app = new AuthApp
            {
                Name = request.Name,
            };
            _authDbContext.Add(app);

            switch (request.DirectoryChoice)
            {
                case AddNewAppRequest.Types.DirectoryChoice.NoneDirectory:
                    app.DirectoryMethod = AuthApp.DirectoryMethodEnum.NONE;
                    break;

                case AddNewAppRequest.Types.DirectoryChoice.LdapDirectory:
                    app.DirectoryMethod = AuthApp.DirectoryMethodEnum.LDAP;
                    break;

                case AddNewAppRequest.Types.DirectoryChoice.ScimDirectory:
                    app.DirectoryMethod = AuthApp.DirectoryMethodEnum.SCIM;
                    break;
            }

            switch (request.AuthChoice)
            {
                case AddNewAppRequest.Types.AuthChoice.LdapAuth:
                    app.AuthMethod = AuthApp.AuthMethodEnum.LDAP;
                    break;

                case AddNewAppRequest.Types.AuthChoice.OidcAuth:
                    app.AuthMethod = AuthApp.AuthMethodEnum.OIDC;
                    break;

                case AddNewAppRequest.Types.AuthChoice.ProxyAuth:
                    app.AuthMethod = AuthApp.AuthMethodEnum.PROXY;
                    break;
            }

            if (app.AuthMethod == AuthApp.AuthMethodEnum.LDAP || app.DirectoryMethod == AuthApp.DirectoryMethodEnum.LDAP)
            {
                string assembledBaseDn = "dc=" + app.Id;
                string bindUserPassword = _ldapSettingsDataProtector.Protect(_secureRandom.GetRandomString(16));
                string bindUser = "cn=BindUser," + assembledBaseDn;

                LdapAppSettings ldapAppSettings = new LdapAppSettings
                {
                    AuthApp = app,
                    UseForAuthentication = (app.AuthMethod == AuthApp.AuthMethodEnum.LDAP),
                    UseForIdentity = (app.DirectoryMethod == AuthApp.DirectoryMethodEnum.LDAP),
                    BaseDn = assembledBaseDn,
                    BindUser = bindUser,
                    BindUserPassword = bindUserPassword,
                };

                _authDbContext.Add(ldapAppSettings);
                app.LdapAppSettings = ldapAppSettings;
            }

            if (app.AuthMethod == AuthApp.AuthMethodEnum.PROXY)
            {
                ProxyAppSettings proxyAppSettings = new ProxyAppSettings
                {
                    AuthApp = app,
                    InternalHostname = request.ProxySetting.InternalHostname,
                    PublicHostname = request.ProxySetting.PublicHostname,
                };
                _authDbContext.Add(proxyAppSettings);
                app.ProxyAppSettings = proxyAppSettings;

                _configurationProvider.TryGet("tls.acme.support", out string isAcmeSupported);
                if (isAcmeSupported == "true")
                {
                    // FIXME: Passing an empty email is a hack here. The email is already passed in InstallService. Could be refactored.
                    BackgroundJob.Enqueue<IRequestAcmeCertificateJob>(job => job.Request("", request.ProxySetting.PublicHostname));
                }
            }

            if (app.AuthMethod == AuthApp.AuthMethodEnum.OIDC)
            {
                OIDCAppSettings oidcAppSettings = new OIDCAppSettings
                {
                    RedirectUrl = request.OidcSetting.RedirectUri,
                    AuthApp = app,
                    ClientId = Guid.NewGuid().ToString(),
                    ClientSecret = _secureRandom.GetRandomString(32),
                    Audience = "FIX_ME",
                };
                _authDbContext.Add(oidcAppSettings);
                app.OidcAppSettings = oidcAppSettings;
            }

            if (app.DirectoryMethod == AuthApp.DirectoryMethodEnum.SCIM)
            {
                SCIMAppSettings scimAppSettings = new SCIMAppSettings
                {
                    AuthApp = app,
                    Endpoint = request.ScimSetting.Endpoint,
                    Credentials = request.ScimSetting.Credentials,
                };
                _authDbContext.Add(scimAppSettings);
                app.ScimAppSettings = scimAppSettings;
            }

            foreach (string groupId in request.GroupIds)
            {
                Guid groupIdGuid = new Guid(groupId);
                UserGroup group = await _authDbContext.UserGroup
                    .Include(g => g.AuthApps)
                    .SingleAsync(g => g.Id == groupIdGuid);
                group.AuthApps.Add(app);
            }

            await _authDbContext.SaveChangesAsync();
            // fixme: this should be done outside a service
            await _memoryPopulator.PopulateFromDatabase();

            return new AddNewAppReply
            {
                Success = true,
            };
        }

        public override async Task<AppDetailReply> GetAppDetails(AppDetailRequest request, ServerCallContext context)
        {
            AuthApp app = await _authDbContext.AuthApp
                .Include(a => a.LdapAppSettings)
                .Include(a => a.UserGroups)
                .Include(a => a.LdapAppSettings)
                .Include(a => a.OidcAppSettings)
                .Include(a => a.ProxyAppSettings)
                .Include(a => a.ScimAppSettings)
                .SingleAsync(f => f.Id == new Guid(request.Id));

            AppDetailReply reply = new AppDetailReply
            {
                Id = app.Id.ToString(),
                Name = app.Name,
            };

            switch (app.DirectoryMethod)
            {
                case AuthApp.DirectoryMethodEnum.NONE:
                    break;
                case AuthApp.DirectoryMethodEnum.LDAP:
                    reply.LdapDirectorySetting = new AppDetailReply.Types.LdapDirectorySetting
                    {
                        BaseDn = app.LdapAppSettings.BaseDn,
                        Password = _ldapSettingsDataProtector.Unprotect(app.LdapAppSettings.BindUserPassword),
                        Username = app.LdapAppSettings.BindUser,
                    };
                    break;
                case AuthApp.DirectoryMethodEnum.SCIM:
                    reply.ScimDirectorySetting = new AppDetailReply.Types.ScimDirectorySetting
                    {
                        Credentials = app.ScimAppSettings.Credentials,
                        Endpoint = app.ScimAppSettings.Endpoint,
                    };
                    break;
            }

            switch (app.AuthMethod)
            {
                case AuthApp.AuthMethodEnum.LDAP:
                    reply.LdapAuthSetting = new AppDetailReply.Types.LdapAuthSetting
                    {
                        BaseDn = app.LdapAppSettings.BaseDn,
                    };
                    break;
                case AuthApp.AuthMethodEnum.OIDC:
                    reply.OidcAuthSetting = new AppDetailReply.Types.OidcAuthSetting
                    {
                        ClientId = app.OidcAppSettings.ClientId,
                        ClientSecret = app.OidcAppSettings.ClientSecret,
                        RedirectUri = app.OidcAppSettings.RedirectUrl,
                    };
                    break;
                case AuthApp.AuthMethodEnum.PROXY:
                    reply.ProxyAuthSetting = new AppDetailReply.Types.ProxyAuthSetting
                    {
                        InternalHostname = app.ProxyAppSettings.InternalHostname,
                        PublicHostname = app.ProxyAppSettings.PublicHostname,
                    };
                    break;
            }

            foreach (UserGroup group in app.UserGroups)
            {
                GrantedAppGroup appGroup = new GrantedAppGroup
                {
                    Id = group.Id.ToString(),
                    Name = group.Name,
                };
                reply.Groups.Add(appGroup);
            }

            return reply;
        }

        public override async Task<AppListReply> ListApps(Empty request, ServerCallContext context)
        {
            AppListReply reply = new AppListReply();

            IEnumerable<AuthApp> apps = await _authDbContext.AuthApp
                .Include(a => a.UserGroups)
                .ToListAsync();

            foreach (AuthApp app in apps)
            {
                AddNewAppRequest.Types.AuthChoice authChoice;
                AddNewAppRequest.Types.DirectoryChoice directoryChoice;

                switch (app.DirectoryMethod)
                {
                    case AuthApp.DirectoryMethodEnum.NONE:
                        directoryChoice = AddNewAppRequest.Types.DirectoryChoice.NoneDirectory;
                        break;
                    case AuthApp.DirectoryMethodEnum.LDAP:
                        directoryChoice = AddNewAppRequest.Types.DirectoryChoice.LdapDirectory;
                        break;
                    case AuthApp.DirectoryMethodEnum.SCIM:
                        directoryChoice = AddNewAppRequest.Types.DirectoryChoice.ScimDirectory;
                        break;
                    default:
                        throw new Exception("Unexpected directory method");
                }

                switch (app.AuthMethod)
                {
                    case AuthApp.AuthMethodEnum.LDAP:
                        authChoice = AddNewAppRequest.Types.AuthChoice.LdapAuth;
                        break;
                    case AuthApp.AuthMethodEnum.OIDC:
                        authChoice = AddNewAppRequest.Types.AuthChoice.OidcAuth;
                        break;
                    case AuthApp.AuthMethodEnum.PROXY:
                        authChoice = AddNewAppRequest.Types.AuthChoice.ProxyAuth;
                        break;
                    default:
                        throw new Exception("Unexpected app method");
                }

                AppListEntry entry = new AppListEntry
                {
                    Id = app.Id.ToString(),
                    Name = app.Name,
                    GroupsAssigned = app.UserGroups.Count(),
                    AuthChoice = authChoice,
                    DirectoryChoice = directoryChoice,
                };

                reply.Apps.Add(entry);
            }

            return reply;
        }

        public override async Task<RemoveGroupFromAppReply> RemoveGroupFromApp(RemoveGroupFromAppRequest request, ServerCallContext context)
        {
            Guid groupId = new Guid(request.GroupId);
            UserGroup group = await _authDbContext.UserGroup
                .SingleAsync(g => g.Id == groupId);

            Guid appId = new Guid(request.AppId);
            AuthApp app = await _authDbContext.AuthApp
                .Include(a => a.UserGroups)
                .SingleAsync(a => a.Id == appId);

            app.UserGroups.Remove(group);
            await _authDbContext.SaveChangesAsync();

            return new RemoveGroupFromAppReply { Success = true };
        }
    }
}
