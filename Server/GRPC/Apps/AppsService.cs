using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using AuthServer.Server.Services.Crypto;
using AuthServer.Server.Services.User;
using AuthServer.Shared.Apps;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Server.GRPC.Apps
{
    [Authorize]
    public class AppsService : AuthServer.Shared.Apps.Apps.AppsBase
    {
        private readonly AuthDbContext _authDbContext;
        private readonly SecureRandom _secureRandom;
        private readonly UserManager _userManager;
        private readonly Hasher _hasher;

        public AppsService(
            AuthDbContext authDbContext,
            SecureRandom secureRandom,
            UserManager userManager,
            Hasher hasher)
        {
            _authDbContext = authDbContext;
            _secureRandom = secureRandom;
            _userManager = userManager;
            _hasher = hasher;
        }

        public override async Task<CreateLdapCredentialReply> CreateLdapCredential(CreateLdapCredentialRequest request, ServerCallContext context)
        {
            Guid appId = new Guid(request.Id);
            LdapAppSettings ldapAppSettings = await _authDbContext.LdapAppSettings
                .SingleAsync(l => l.AuthApp.Id == appId);
            AppUser user = await _userManager.GetUserAsync(context.GetHttpContext().User);

            string plainTextPassword = _secureRandom.GetRandomString(16);
            string hashedPassword = _hasher.Hash(plainTextPassword);

            LdapAppUserCredentials credentials = new LdapAppUserCredentials
            {
                HashedPassword = hashedPassword,
                LdapAppSettings = ldapAppSettings,
                User = user,
            };
            _authDbContext.Add(credentials);
            await _authDbContext.SaveChangesAsync();

            return new CreateLdapCredentialReply { Password = plainTextPassword };
        }

        public override async Task<GetAppDetailsReply> GetAppDetails(GetAppDetailsRequest request, ServerCallContext context)
        {
            Guid appId = new Guid(request.Id);
            AuthApp app = await _authDbContext.AuthApp
                .SingleAsync(a => a.Id == appId);

            return new GetAppDetailsReply
            {
                Id = app.Id.ToString(),
                Name = app.Name,
            };
        }

        public override async Task<Shared.Apps.AppListReply> ListApps(Empty request, ServerCallContext context)
        {
            AppListReply reply = new AppListReply();

            IEnumerable<AuthApp> authApps = await _authDbContext.AuthApp
                .ToListAsync();

            foreach (AuthApp app in authApps)
            {
                reply.Apps.Add(new AppListEntry
                {
                    Id = app.Id.ToString(),
                    Name = app.Name,
                });
            }

            return reply;
        }
    }
}
