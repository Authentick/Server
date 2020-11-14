using System.Collections.Generic;
using System.Linq;
using AuthServer.Server.Models;
using AuthServer.Server.Services.User;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace AuthServer.Server.Tests.Services.User
{
    public class UserManagerTest : IClassFixture<SharedDatabaseFixture>
    {
        public UserManagerTest(SharedDatabaseFixture fixture)
        {
            Fixture = fixture;
        }

        public SharedDatabaseFixture Fixture { get; }

        [Fact]
        public void GetAllUsers()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                {
                    UserManager manager = new UserManager((new Mock<IUserStore<AppUser>>()).Object, null, null, null, null, null, null, null, null, context);

                    Assert.Collection(
                        manager.GetAllUsers(),
                        item => Assert.Equal("Test User 1", item.UserName),
                        item => Assert.Equal("Test User 2", item.UserName)
                    );
                    Assert.Equal(2, manager.GetAllUsers().Count());
                }
            }
        }
    }
}
