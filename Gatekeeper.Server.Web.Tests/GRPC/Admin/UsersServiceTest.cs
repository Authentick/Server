using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AuthServer.Server.GRPC.Admin;
using AuthServer.Server.Models;
using AuthServer.Server.Services.User;
using AuthServer.Shared.Admin;
using Gatekeeper.Server.Services.FileStorage;
using Grpc.Core;
using Grpc.Core.Testing;
using Grpc.Core.Utils;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace AuthServer.Server.Tests.GRPC.Admin
{
    public class UsersServiceTest
    {
        [Fact]
        public async Task ListUsersAsync()
        {

            var userManagerMock = new Mock<UserManager>((new Mock<IUserStore<AppUser>>()).Object, null, null, null, null, null, null, null, null, null);
            var profileManagerMock = new Mock<ProfileImageManager>();
            userManagerMock.Setup(m => m.GetAllUsers()).Returns(new List<AppUser>{
                new AppUser{
                    Id = new Guid("68cfbe00-8e8d-4015-8bb5-6ce82ff4d64d"),
                    UserName = "User 1", 
                    Email="test1@example.com"
                },
                new AppUser{
                    Id = new Guid("917cf76f-a076-4cd0-93ef-6c26025f21c0"), 
                    UserName = "User 2", 
                    Email="test2@example.com"
                }
            });

            UsersService service = new UsersService(userManagerMock.Object, profileManagerMock.Object);
            UserListReply actualReply = await service.ListUsers(new Google.Protobuf.WellKnownTypes.Empty(), TestServerCallContext.Create("fooMethod", null, DateTime.UtcNow.AddHours(1), new Metadata(), CancellationToken.None, "127.0.0.1", null, null, (metadata) => TaskUtils.CompletedTask, () => new WriteOptions(), (writeOptions) => { }));

            UserListReply expectedReply = new UserListReply();
            expectedReply.Users.Add(new User
            {
                Id = "68cfbe00-8e8d-4015-8bb5-6ce82ff4d64d",
                Name = "User 1",
                Email = "test1@example.com",
                IsEnabled = true,
            });
            expectedReply.Users.Add(new User
            {
                Id = "917cf76f-a076-4cd0-93ef-6c26025f21c0",
                Name = "User 2",
                Email = "test2@example.com",
                IsEnabled = true,
            });

            Assert.Equal(expectedReply, actualReply);
        }
    }
}