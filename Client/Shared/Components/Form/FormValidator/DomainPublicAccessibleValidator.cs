using System.Threading;
using System.Threading.Tasks;
using AuthServer.Shared;
using static AuthServer.Shared.ConnectivityCheckService;

namespace AuthServer.Client.Shared.Components.Form.FormValidator
{
    public class DomainPublicAccessibleValidator : IFormValidator
    {
        private readonly ConnectivityCheckServiceClient _connectivityCheckServiceClient;

        public DomainPublicAccessibleValidator(ConnectivityCheckServiceClient connectivityCheckServiceClient)
        {
            _connectivityCheckServiceClient = connectivityCheckServiceClient;
        }

        public async Task<FormValidatorResponse> Check(string value, CancellationToken cancellationToken)
        {
            IsPublicAccessibleRequest request = new IsPublicAccessibleRequest
            {
                Hostname = value,
            };

            try
            {
                IsPublicAccessibleReply reply = await _connectivityCheckServiceClient.IsPublicAccessibleAsync(request, null, null, cancellationToken);
                if (reply.State == IsPublicAccessibleReply.Types.AccessibleReplyEnum.Success)
                {
                    return new FormValidatorResponse(true, null);
                }
            }
            catch { }

            return new FormValidatorResponse(false, "Domain does not point to this server");
        }
    }
}
