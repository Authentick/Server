using System;
using System.Threading;
using System.Threading.Tasks;

namespace Gatekeeper.Client.Shared.Components.Form.FormValidator
{
    public class PortValidator : IFormValidator
    {
        public Task<FormValidatorResponse> Check(string value, CancellationToken cancellationToken)
        {
            try {
                int port = Int32.Parse(value);
                if(port >= 1 && port <= 65535)
                {
                    return Task.FromResult(new FormValidatorResponse(true, null));
                }
            } catch (FormatException) {}

            return Task.FromResult(new FormValidatorResponse(false, "Not a valid port number"));
        }
    }
}
