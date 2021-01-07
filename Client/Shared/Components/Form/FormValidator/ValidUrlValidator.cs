using System;
using System.Threading;
using System.Threading.Tasks;

namespace AuthServer.Client.Shared.Components.Form.FormValidator
{
    public class ValidUrlValidator : IFormValidator
    {
        public Task<FormValidatorResponse> Check(string value, CancellationToken cancellationToken)
        {
            if (Uri.IsWellFormedUriString(value, UriKind.Absolute))
            {
                return Task.FromResult(new FormValidatorResponse(true, null));
            }

            return Task.FromResult(new FormValidatorResponse(false, "URL is not valid"));
        }
    }
}
