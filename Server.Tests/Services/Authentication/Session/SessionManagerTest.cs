using System;
using System.Linq;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using AuthServer.Server.Services.Authentication.Session;
using NodaTime;
using Xunit;

namespace AuthServer.Server.Tests.Services.Authentication.Session
{
    public class SessionManagerTest : IClassFixture<SharedDatabaseFixture>
    {
        public SessionManagerTest(SharedDatabaseFixture fixture)
        {
            Fixture = fixture;
        }

        public SharedDatabaseFixture Fixture { get; }

        [Fact]
        public void MarkSessionLastUsedNow_already_updated_in_last_minute()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                {
                    SessionManager manager = new SessionManager(context);

                    Instant lastUsedTime = SystemClock.Instance.GetCurrentInstant();

                    AuthSession session = new AuthSession
                    {
                        Name = "TestSession",
                        LastUsedTime = lastUsedTime,
                        User = context.Users.Single(u => u.Email == "test1@example.com")
                    };
                    context.Add(session);
                    context.SaveChanges();

                    manager.MarkSessionLastUsedNow(session);

                    AuthSession newLoadedTime = context.AuthSessions.Single(s => s.Id == session.Id);

                    Assert.Equal(lastUsedTime, newLoadedTime.LastUsedTime);
                }

                using (var context = Fixture.CreateContext(transaction))
                {
                    SessionManager manager = new SessionManager(context);

                    Instant lastUsedTime = (SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromMinutes(2)));

                    AuthSession session = new AuthSession
                    {
                        Name = "TestSession",
                        LastUsedTime = lastUsedTime,
                        User = context.Users.Single(u => u.Email == "test1@example.com")
                    };
                    context.Add(session);
                    context.SaveChanges();

                    manager.MarkSessionLastUsedNow(session);

                    AuthSession newLoadedTime = context.AuthSessions.Single(s => s.Id == session.Id);

                    Assert.NotEqual(lastUsedTime, newLoadedTime.LastUsedTime);
                }
            }
        }

        [Fact]
        public void MarkSessionLastUsedNow_not_updated_in_last_minute()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                {
                    SessionManager manager = new SessionManager(context);

                    Instant lastUsedTime = (SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromMinutes(2)));

                    AuthSession session = new AuthSession
                    {
                        Name = "TestSession",
                        LastUsedTime = lastUsedTime,
                        User = context.Users.Single(u => u.Email == "test1@example.com")
                    };
                    context.Add(session);
                    context.SaveChanges();

                    manager.MarkSessionLastUsedNow(session);

                    AuthSession newLoadedTime = context.AuthSessions.Single(s => s.Id == session.Id);

                    Assert.NotEqual(lastUsedTime, newLoadedTime.LastUsedTime);
                }
            }
        }

        [Fact]
        public async Task IsSessionActive_for_active_session_and_correct_user()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                {
                    SessionManager manager = new SessionManager(context);

                    Instant lastUsedTime = (SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromMinutes(2)));
                    AppUser user = context.Users.Single(u => u.Email == "test1@example.com");

                    AuthSession session = new AuthSession
                    {
                        Name = "TestSession",
                        LastUsedTime = lastUsedTime,
                        User = user
                    };
                    context.Add(session);
                    context.SaveChanges();

                    AuthSession readSession = await manager.GetActiveSessionById(user.Id, session.Id);

                    Assert.Equal(session, readSession);
                }
            }
        }

        [Fact]
        public async Task IsSessionActive_for_active_session_and_incorrect_user()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                {
                    SessionManager manager = new SessionManager(context);

                    Instant lastUsedTime = (SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromMinutes(2)));
                    AppUser user1 = context.Users.Single(u => u.Email == "test1@example.com");
                    AppUser user2 = context.Users.Single(u => u.Email == "test2@example.com");

                    AuthSession session = new AuthSession
                    {
                        Name = "TestSession",
                        LastUsedTime = lastUsedTime,
                        User = user1
                    };
                    context.Add(session);
                    context.SaveChanges();

                    AuthSession readSession = await manager.GetActiveSessionById(user2.Id, session.Id);

                    Assert.Null(readSession);
                }
            }
        }

        [Fact]
        public async Task IsSessionActive_for_expired_session_and_correct_user()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                {
                    SessionManager manager = new SessionManager(context);

                    Instant lastUsedTime = (SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromMinutes(2)));
                    AppUser user = context.Users.Single(u => u.Email == "test1@example.com");

                    AuthSession session = new AuthSession
                    {
                        Name = "TestSession",
                        LastUsedTime = lastUsedTime,
                        User = user
                    };
                    context.Add(session);
                    context.SaveChanges();

                    manager.ExpireSession(user, session.Id);

                    AuthSession readSession = await manager.GetActiveSessionById(user.Id, session.Id);

                    Assert.Null(readSession);
                }
            }
        }

        [Fact]
        public void IsSessionActive_for_expired_session_and_incorrect_user()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                {
                    SessionManager manager = new SessionManager(context);

                    Instant lastUsedTime = (SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromMinutes(2)));
                    AppUser user1 = context.Users.Single(u => u.Email == "test1@example.com");
                    AppUser user2 = context.Users.Single(u => u.Email == "test2@example.com");

                    AuthSession session = new AuthSession
                    {
                        Name = "TestSession",
                        LastUsedTime = lastUsedTime,
                        User = user1
                    };
                    context.Add(session);
                    context.SaveChanges();

                    Assert.Throws<InvalidOperationException>(() => manager.ExpireSession(user2, session.Id));
                }
            }
        }
    }
}
