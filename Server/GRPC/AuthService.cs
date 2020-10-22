using System.Threading.Tasks;
using AuthServer.Shared;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AuthServer.Server.GRPC
{
    public class AuthService : AuthServer.Shared.Auth.AuthBase
    {
        public override Task<WhoAmIReply> WhoAmI(Empty request, ServerCallContext context)
        {
            var result = new WhoAmIReply { IsAuthenticated = true, UserId = "foobar" };
            return Task.FromResult(result);
        }
    }
}
