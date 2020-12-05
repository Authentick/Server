using System;
using System.Threading;
using System.Threading.Tasks;
using AuthServer.Server.GRPC;
using AuthServer.Shared;
using AuthServer.Shared.Admin;
using Grpc.Core;
using Grpc.Core.Testing;
using Grpc.Core.Utils;
using Moq;
using Xunit;

namespace AuthServer.Server.Tests.GRPC
{
    public class TypeaheadServiceTest : IClassFixture<SharedDatabaseFixture>
    {
        public readonly SharedDatabaseFixture Fixture;

        public TypeaheadServiceTest(SharedDatabaseFixture fixture)
        {
            Fixture = fixture;
        }

        [Fact]
        public async Task SearchUser()
        {
            using (var transaction = Fixture.Connection.BeginTransaction())
            {
                using (var context = Fixture.CreateContext(transaction))
                {
                    TypeaheadService typeaheadService = new TypeaheadService(context);

                    SearchUserRequest request = new SearchUserRequest
                    {
                        SearchParameter = "Test User",
                    };

                    SearchUserReply actualReply = await typeaheadService.SearchUser(request, TestServerCallContext.Create("fooMethod", "test.example.com", DateTime.UtcNow.AddHours(1), new Metadata(), CancellationToken.None, "127.0.0.1", null, null, (metadata) => TaskUtils.CompletedTask, () => new WriteOptions(), (writeOptions) => { }));
                    Assert.Equal(2, actualReply.Entries.Count);
                }
            }
        }
    }
}
