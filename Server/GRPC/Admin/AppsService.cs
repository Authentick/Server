using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using AuthServer.Server.Services.Crypto;
using AuthServer.Shared.Admin;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Server.GRPC.Admin
{
    [Authorize]
    public class AppsService : AuthServer.Shared.Admin.AdminApps.AdminAppsBase
    {
        private readonly AuthDbContext _authDbContext;
        private readonly IDataProtector _ldapSettingsDataProtector;
        private readonly SecureRandom _secureRandom;

        public AppsService(
            AuthDbContext authDbContext,
            IDataProtectionProvider dataProtectionProvider,
            SecureRandom secureRandom
            )
        {
            _authDbContext = authDbContext;
            _ldapSettingsDataProtector = dataProtectionProvider.CreateProtector("LdapSettingsDataProtector");
            _secureRandom = secureRandom;
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
                LdapAppSettings ldapAppSettings = new LdapAppSettings
                {
                    AuthApp = app,
                    UseForAuthentication = (app.AuthMethod == AuthApp.AuthMethodEnum.LDAP),
                    UseForIdentity = (app.DirectoryMethod == AuthApp.DirectoryMethodEnum.LDAP),
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
            }

            if (app.AuthMethod == AuthApp.AuthMethodEnum.OIDC)
            {
                OIDCAppSettings oidcAppSettings = new OIDCAppSettings
                {
                    RedirectUrl = request.OidcSetting.RedirectUri,
                    AuthApp = app,
                };
                _authDbContext.Add(oidcAppSettings);
                app.OidcAppSettings = oidcAppSettings;
            }

            if (app.DirectoryMethod == AuthApp.DirectoryMethodEnum.SCIM)
            {
                SCIMAppSettings scimAppSettings = new SCIMAppSettings
                {
                    AuthApp = app,
                    Hostname = request.ScimSetting.Hostname,
                };
                _authDbContext.Add(scimAppSettings);
                app.ScimAppSettings = scimAppSettings;
            }

            foreach (string groupId in request.GroupIds)
            {
                Guid groupIdGuid = new Guid(groupId);
                UserGroup group = await _authDbContext.UserGroup
                    .SingleAsync(g => g.Id == groupIdGuid);
                app.UserGroups.Add(group);
            }

            await _authDbContext.SaveChangesAsync();

            return new AddNewAppReply
            {
                Success = true,
            };
        }

        public override Task<AppDetailReply> GetAppDetails(AppDetailRequest request, ServerCallContext context)
        {
            AuthApp app = _authDbContext.AuthApp
                .Include(a => a.LdapAppSettings)
                .Include(a => a.UserGroups)
                .Single(f => f.Id == new Guid(request.Id));

            AppDetailReply reply = new AppDetailReply
            {
                Id = app.Id.ToString(),
                LdapBindCredentials = (app.LdapAppSettings != null) ? app.LdapAppSettings.BindUser : "",
                LdapBindCredentialsPassword = (app.LdapAppSettings != null) ? _ldapSettingsDataProtector.Unprotect(app.LdapAppSettings.BindUserPassword) : "",
                LdapDn = (app.LdapAppSettings != null) ? app.LdapAppSettings.BaseDn : "",
                Name = app.Name,
            };

            foreach (UserGroup group in app.UserGroups)
            {
                GrantedAppGroup appGroup = new GrantedAppGroup
                {
                    Id = group.Id.ToString(),
                    Name = group.Name,
                };
                reply.Groups.Add(appGroup);
            }

            return Task.FromResult(reply);
        }

        public override Task<AppListReply> ListApps(Empty request, ServerCallContext context)
        {
            AppListReply reply = new AppListReply();

            IEnumerable<AuthApp> apps = _authDbContext.AuthApp.ToList();

            foreach (AuthApp app in apps)
            {
                AppListEntry entry = new AppListEntry
                {
                    Id = app.Id.ToString(),
                    Name = app.Name,
                };

                reply.Apps.Add(entry);
            }

            return Task.FromResult(reply);
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
