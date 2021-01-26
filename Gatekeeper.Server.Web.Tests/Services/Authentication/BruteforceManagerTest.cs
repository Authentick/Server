using System.Net;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using AuthServer.Server.Services.Authentication;
using AuthServer.Server.Services.User;
using Gatekeeper.Server.Web.Services.Alerts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AuthServer.Server.Tests.Services.Authentication.Session
{
    public class BruteforceManagerTest : IClassFixture<SharedDatabaseFixture>
    {
        public BruteforceManagerTest(SharedDatabaseFixture fixture)
        {
            Fixture = fixture;
        }

        public SharedDatabaseFixture Fixture { get; }

        [Fact]
        public async Task BruteforceBlockIsWorking()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                {
                    Mock<UserManager> userManagerMock = new Mock<UserManager>((new Mock<IUserStore<AppUser>>()).Object, null, null, null, null, null, null, null, null, null);
                    AppUser returnedUser = await context.Users.SingleAsync(u => u.Email == "test1@example.com");
                    userManagerMock.Setup(u => u.FindByNameAsync("JohnDoe"))
                        .ReturnsAsync(returnedUser);

                    Mock<AlertManager> alertManagerMock = new Mock<AlertManager>(context);

                    BruteforceManager manager = new BruteforceManager(
                        context,
                        userManagerMock.Object,
                        alertManagerMock.Object
                    );

                    IPAddress ip = IPAddress.Parse("192.168.5.34");
                    IPAddress otherIp = IPAddress.Parse("192.168.5.35");

                    await manager.MarkInvalidLoginAttemptAsync(ip, "User Agent", "JohnDoe");

                    Assert.False(await manager.IsIpBlockedAsync(ip));
                    Assert.False(await manager.IsUserBlockedAsync(returnedUser));

                    for (int i = 1; i < 10; i++)
                    {
                        await manager.MarkInvalidLoginAttemptAsync(ip, "User Agent", "JohnDoe");
                    }

                    Assert.True(await manager.IsIpBlockedAsync(ip));
                    Assert.False(await manager.IsIpBlockedAsync(otherIp));
                    Assert.True(await manager.IsUserBlockedAsync(returnedUser));
                }
            }
        }
    }
}
