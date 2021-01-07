using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AuthServer.Client.Shared.Components.Form.FormValidator
{
    public class PasswordPolicyValidator : IFormValidator
    {
        public Task<FormValidatorResponse> Check(string value, CancellationToken cancellationToken)
        {
            if (
                value.Length < 6 ||
                !value.Any(char.IsUpper) ||
                !value.Any(char.IsLower) ||
                !value.Any(char.IsDigit) ||
                value.All(char.IsLetterOrDigit)
            )
            {
                return Task.FromResult(new FormValidatorResponse(false, "Input does not meet minimum password criteria"));
            }

            return Task.FromResult(new FormValidatorResponse(true, null));
        }
    }
}
