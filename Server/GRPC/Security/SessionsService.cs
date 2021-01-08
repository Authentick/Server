using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using AuthServer.Server.Services.Authentication.Session;
using AuthServer.Server.Services.User;
using AuthServer.Shared.Security;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using NodaTime;

namespace AuthServer.Server.GRPC.Security
{
    [Authorize]
    public class SessionsService : AuthServer.Shared.Security.Sessions.SessionsBase
    {
        private readonly UserManager _userManager;
        private readonly SessionManager _sessionManager;

        public SessionsService(
            UserManager userManager,
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
                    LastActive = NodaTime.Serialization.Protobuf.NodaExtensions.ToTimestamp(session.LastUsedTime),
                    Name = session.Name,
                    SignedIn = NodaTime.Serialization.Protobuf.NodaExtensions.ToTimestamp(session.CreationTime),
                };

                foreach (AuthSessionIp sessionIp in session.SessionIps)
                {
                    Session.Types.LocationReply locationReply = new Session.Types.LocationReply
                    {
                        IpAddress = sessionIp.IpAddress.ToString(),
                        Country = sessionIp.Country,
                        City = sessionIp.City,
                    };
                    replySession.Locations.Add(locationReply);
                }

                Instant? expiredTime = session.ExpiredTime;
                if (expiredTime != null)
                {
                    replySession.InvalidatedAt = NodaTime.Serialization.Protobuf.NodaExtensions.ToTimestamp((Instant)expiredTime);
                }

                reply.Sessions.Add(replySession);
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