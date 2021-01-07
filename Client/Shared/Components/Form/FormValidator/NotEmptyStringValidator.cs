using System.Threading;
using System.Threading.Tasks;

namespace AuthServer.Client.Shared.Components.Form.FormValidator
{
    public class NotEmptyStringValidator : IFormValidator
    {
        public Task<FormValidatorResponse> Check(string value, CancellationToken cancellationToken)
        {
            if(value.Length > 0) {
                return Task.FromResult(new FormValidatorResponse(true, null));
            }

            return Task.FromResult(new FormValidatorResponse(false, "Input cannot be empty"));
        }
    }
}
