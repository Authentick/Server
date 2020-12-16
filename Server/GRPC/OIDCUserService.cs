using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using AuthServer.Server.Services.User;
using AuthServer.Shared;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace AuthServer.Server.GRPC
{
    [Authorize]
    public class OIDCUserService : AuthServer.Shared.OIDCUserService.OIDCUserServiceBase
    {
        private readonly AuthDbContext _authDbContext;
        private readonly UserManager _userManager;

        public OIDCUserService(
            AuthDbContext authDbContext,
            UserManager userManager)
        {
            _authDbContext = authDbContext;
            _userManager = userManager;
        }

        public override async Task<GrantApplicationReply> GrantApplication(GrantApplicationRequest request, ServerCallContext context)
        {
            AppUser currentUser = await _userManager.GetUserAsync(context.GetHttpContext().User);

            List<UserGroup> currentUserGroups = await _authDbContext.UserGroup
                .AsNoTracking()
                .Where(u => u.Members.Contains(currentUser))
                .ToListAsync();

            AuthApp app = await _authDbContext.AuthApp
                .AsNoTracking()
                .Include(a => a.OidcAppSettings)
                .SingleAsync(a => a.Id == new Guid(request.AppId) && a.UserGroups.Any());

            if (app.OidcAppSettings == null)
            {
                return new GrantApplicationReply
                {
                    Success = false,
                };
            }

            if (app.OidcAppSettings.RedirectUrl != request.RedirectUri)
            {
                return new GrantApplicationReply
                {
                    Success = false
                };
            }

            OIDCSession session = new OIDCSession
            {
                CreationTime = SystemClock.Instance.GetCurrentInstant(),
                OIDCAppSettings = app.OidcAppSettings,
                User = currentUser,
            };
            _authDbContext.Add(session);
            await _authDbContext.SaveChangesAsync();

            // TODO: encrypt this
            string accessToken = session.Id.ToString();

            return new GrantApplicationReply
            {
                Success = true,
                AccessToken = accessToken,
            };
        }
    }
}
