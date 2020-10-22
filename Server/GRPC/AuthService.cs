using System.Threading.Tasks;
using AuthServer.Shared;
using Grpc.Core;

namespace AuthServer.Server.GRPC
{
    public class AuthService : AuthServer.Shared.Auth.AuthBase
    {
        public override async Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return new HelloReply { Message = "Test" };
        }
    }
}
