using System.Threading.Tasks;
using AuthServer.Shared.Client;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace AuthServer.Server.GRPC {
    public class ClientService : AuthServer.Shared.Client.Client.ClientBase {
        public override Task<ClientCreationResponse> Add(ClientCreationRequest request, ServerCallContext context)
        {
            return base.Add(request, context);
        }

        public override Task<ClientListResults> List(Empty request, ServerCallContext context)
        {
            return base.List(request, context);
        }
    }
}