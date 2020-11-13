using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using AuthServer.Server.Services.Authentication.Session;
using AuthServer.Shared.Security;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace AuthServer.Server.GRPC.Security
{
    [Authorize]
    public class SessionsService : AuthServer.Shared.Security.Sessions.SessionsBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SessionManager _sessionManager;

        public SessionsService(
            UserManager<AppUser> userManager,
            SessionManager sessionManager
            )
        {
            _userManager = userManager;
            _sessionManager = sessionManager;
        }

        public override async Task<InvalidateSessionReply> InvalidateSession(InvalidateSessionRequest request, ServerCallContext context)
        {
            AppUser user = await _userManager.GetUserAsync(context.GetHttpContext().User);
            Guid sessionId = new Guid(request.Id);
            _sessionManager.ExpireSession(user, sessionId);

            return new InvalidateSessionReply { Success = true };
        }

        private SessionListReply FormatSessionListReply(List<AuthSession> sessions)
        {
            SessionListReply reply = new SessionListReply();

            foreach (AuthSession session in sessions)
            {
                Session replySession = new Session
                {
                    Id = session.Id.ToString(),
                    LastActive = "TODO",
                    LastLocation = "TODO",
                    Name = "TODO",
                    SignedIn = session.CreationTime.ToString()
                };

                reply.Session.Add(replySession);
            }

            return reply;
        }

        public override async Task<SessionListReply> ListActiveSessions(Empty request, ServerCallContext context)
        {
            AppUser user = await _userManager.GetUserAsync(context.GetHttpContext().User);
            List<AuthSession> sessions = _sessionManager.GetActiveSessionsForUser(user);

            return FormatSessionListReply(sessions);
        }

        public override async Task<SessionListReply> ListInactiveSessions(Empty request, ServerCallContext context)
        {
            AppUser user = await _userManager.GetUserAsync(context.GetHttpContext().User);
            List<AuthSession> sessions = _sessionManager.GetExpiredSessionsForUser(user);

            return FormatSessionListReply(sessions);
        }
    }
}