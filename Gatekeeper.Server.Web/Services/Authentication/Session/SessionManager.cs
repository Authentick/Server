using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
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

        public async Task<AuthSession?> GetActiveSessionById(Guid userId, Guid sessionId)
        {
            return await _authDbContext.AuthSessions
                .SingleOrDefaultAsync(
                    s => 
                    s.User.Id == userId &&
                    s.User.IsDisabled == false &&
                    s.Id == sessionId && 
                    s.ExpiredTime == null
                );
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
                .Include(s => s.SessionIps)
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
                .Include(s => s.SessionIps)
                .Where(
                        s => (s.User == user && s.ExpiredTime != null)
                    )
                .ToList();

            return sessions;
        }

        public void MarkSessionLastUsedNow(AuthSession session)
        {
            Duration duration = Duration.FromMinutes(1);

            if ((SystemClock.Instance.GetCurrentInstant().Minus(duration)) > session.LastUsedTime)
            {
                session.LastUsedTime = SystemClock.Instance.GetCurrentInstant();
                _authDbContext.SaveChanges();
            }
        }
    }
}
