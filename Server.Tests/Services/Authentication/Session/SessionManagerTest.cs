using System.Linq;
using AuthServer.Server.Models;
using AuthServer.Server.Services.Authentication.Session;
using NodaTime;
using Xunit;

namespace AuthServer.Server.Tests.Services.Authentication.Session
{
    public class SharedDatabaseTest : IClassFixture<SharedDatabaseFixture>
    {
        public SharedDatabaseTest(SharedDatabaseFixture fixture)
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
                        User = context.Users.Single(u => u.Email == "test@example.com")
                    };
                    context.Add(session);
                    context.SaveChanges();

                    manager.MarkSessionLastUsedNow(session.Id);

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
                        User = context.Users.Single(u => u.Email == "test@example.com")
                    };
                    context.Add(session);
                    context.SaveChanges();

                    manager.MarkSessionLastUsedNow(session.Id);

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
                        User = context.Users.Single(u => u.Email == "test@example.com")
                    };
                    context.Add(session);
                    context.SaveChanges();

                    manager.MarkSessionLastUsedNow(session.Id);

                    AuthSession newLoadedTime = context.AuthSessions.Single(s => s.Id == session.Id);

                    Assert.NotEqual(lastUsedTime, newLoadedTime.LastUsedTime);
                }
            }
        }
    }
}