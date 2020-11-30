using System;
using System.Threading;
using System.Threading.Tasks;
using AuthServer.Server.GRPC.Admin;
using AuthServer.Server.Models;
using AuthServer.Server.Services.Crypto;
using AuthServer.Shared.Admin;
using Grpc.Core;
using Grpc.Core.Testing;
using Grpc.Core.Utils;
using Microsoft.AspNetCore.DataProtection;
using Moq;
using Xunit;

namespace AuthServer.Server.Tests.GRPC.Admin
{
    public class AppsServiceTest : IClassFixture<SharedDatabaseFixture>
    {
        public readonly SharedDatabaseFixture Fixture;
        private Mock<IDataProtectionProvider> _protectionProviderMock;
        private Mock<IDataProtector> _protectorMock;

        public AppsServiceTest(SharedDatabaseFixture fixture)
        {
            Fixture = fixture;
        }

        private AppsService GetAppsService(AuthDbContext dbContext)
        {
            _protectorMock = new Mock<IDataProtector>();

            _protectionProviderMock = new Mock<IDataProtectionProvider>();
            _protectionProviderMock.Setup(m => m.CreateProtector("LdapSettingsDataProtector"))
                .Returns(_protectorMock.Object);

            Mock<SecureRandom> secureRandomMock = new Mock<SecureRandom>();

            return new AppsService(
                dbContext,
                _protectionProviderMock.Object,
                secureRandomMock.Object
            );
        }

        [Fact]
        public async Task AddNewApp()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                {
                    AppsService appsService = GetAppsService(context);

                    AddNewAppRequest request = new AddNewAppRequest
                    {
                        Name = "My App",
                        HasLdapAuth = true,
                        HasLdapDirectory = true,
                    };

                    AddNewAppReply actualReply = await appsService.AddNewApp(request, TestServerCallContext.Create("fooMethod", "test.example.com", DateTime.UtcNow.AddHours(1), new Metadata(), CancellationToken.None, "127.0.0.1", null, null, (metadata) => TaskUtils.CompletedTask, () => new WriteOptions(), (writeOptions) => { }));
                    Assert.True(actualReply.Success);
                }
            }
        }

        [Fact]
        public async Task GetAppDetails()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                {
                    AuthApp app = new AuthApp
                    {
                        Name = "My App",
                    };
                    context.AuthApp.Add(app);
                    await context.SaveChangesAsync();

                    AppsService appsService = GetAppsService(context);

                    AppDetailRequest request = new AppDetailRequest
                    {
                        Id = app.Id.ToString(),
                    };

                    AppDetailReply expected = new AppDetailReply
                    {
                        Id = app.Id.ToString(),
                        Name = "My App",
                        LdapBindCredentials = "",
                        LdapDn = "",
                    };

                    AppDetailReply actualReply = await appsService.GetAppDetails(request, TestServerCallContext.Create("fooMethod", null, DateTime.UtcNow.AddHours(1), new Metadata(), CancellationToken.None, "127.0.0.1", null, null, (metadata) => TaskUtils.CompletedTask, () => new WriteOptions(), (writeOptions) => { }));
                    Assert.Equal(expected, actualReply);
                }
            }
        }

        [Fact]
        public async Task ListApps()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                {
                    AuthApp app = new AuthApp
                    {
                        Name = "My App",
                    };
                    context.AuthApp.Add(app);
                    await context.SaveChangesAsync();

                    AppsService appsService = GetAppsService(context);

                    AppListReply expected = new AppListReply();
                    expected.Apps.Add(new AppListEntry
                    {
                        Id = app.Id.ToString(),
                        Name = "My App",
                    });
                    AppListReply actualReply = await appsService.ListApps(new Google.Protobuf.WellKnownTypes.Empty(), TestServerCallContext.Create("fooMethod", null, DateTime.UtcNow.AddHours(1), new Metadata(), CancellationToken.None, "127.0.0.1", null, null, (metadata) => TaskUtils.CompletedTask, () => new WriteOptions(), (writeOptions) => { }));
                    Assert.Equal(expected, actualReply);
                }
            }
        }
    }
}
