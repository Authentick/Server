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
    public class AppsService : AuthServer.Shared.Admin.Apps.AppsBase
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

            if (request.HasLdapAuth || request.HasLdapDirectory)
            {
                string assembledBaseDn = "dc=" + app.Id;
                string password = _ldapSettingsDataProtector.Protect(_secureRandom.GetRandomString(16));

                LdapAppSettings ldapAppSettings = new LdapAppSettings
                {
                    AuthApp = app,
                    BaseDn = assembledBaseDn,
                    BindUser = "cn=BindUser," + assembledBaseDn,
                    BindUserPassword = password,
                    UseForAuthentication = request.HasLdapAuth,
                    UseForIdentity = request.HasLdapDirectory,
                };
                _authDbContext.Add(ldapAppSettings);
            }

            await _authDbContext.SaveChangesAsync();

            return new AddNewAppReply { Success = true };
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
