using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using AuthServer.Server.Models;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace AuthServer.Server.Services.Authentication.Session
{
    public class SessionManager
    {
        private readonly AuthDbContext _authDbContext;

        public SessionManager(AuthDbContext authDbContext)
        {
            _authDbContext = authDbContext;
        }

        public Guid GetCurrentSessionId(ClaimsPrincipal principal)
        {
            return new Guid(principal.Claims.Single(u => u.Type == "cookie_identifier").Value);
        }

        public bool IsSessionActive(AppUser user, Guid sessionId)
        {
            AuthSession? session = _authDbContext.AuthSessions
                .AsNoTracking()
                .SingleOrDefault(s => s.User == user && s.Id == sessionId && s.ExpiredTime == null);

            return session != null;
        }

        public void ExpireSession(AppUser user, Guid sessionId)
        {
            AuthSession session = _authDbContext.AuthSessions
                .Single(s => s.User == user && s.Id == sessionId && s.ExpiredTime == null);
            session.ExpiredTime = SystemClock.Instance.GetCurrentInstant();

            _authDbContext.SaveChanges();
        }

        public List<AuthSession> GetActiveSessionsForUser(AppUser user)
        {
            List<AuthSession> sessions = _authDbContext.AuthSessions
                .AsNoTracking()
                .Where(
                        s => (s.User == user && s.ExpiredTime == null)
                    )
                .ToList();

            return sessions;
        }

        public List<AuthSession> GetExpiredSessionsForUser(AppUser user)
        {
            List<AuthSession> sessions = _authDbContext.AuthSessions
                .AsNoTracking()
                .Where(
                        s => (s.User == user && s.ExpiredTime != null)
                    )
                .ToList();

            return sessions;
        }
    }
}
