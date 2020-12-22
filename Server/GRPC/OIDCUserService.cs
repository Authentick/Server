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

            OIDCAppSettings settings = await _authDbContext.OIDCAppSettings
                .Where(u => u.ClientId == request.AppId)
               // FIXME: add this condition
               // .Where(u => u.RedirectUrl == request.RedirectUri)
                .Where(u => u.AuthApp.UserGroups.Any(u => u.Members.Contains(currentUser)))
                .SingleAsync();

            OIDCSession session = new OIDCSession
            {
                CreationTime = SystemClock.Instance.GetCurrentInstant(),
                OIDCAppSettings = settings,
                User = currentUser,
                Nonce = request.Nonce,
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
