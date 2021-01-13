using System.Threading;
using System.Threading.Tasks;
using AuthServer.Shared;
using static AuthServer.Shared.Auth;

namespace Gatekeeper.Client.Shared.Components.Form.FormValidator
{
    public class PasswordPolicyValidator : IFormValidator
    {
        private readonly AuthClient _authClient;

        public PasswordPolicyValidator(AuthClient authClient)
        {
            _authClient = authClient;
        }

        public async Task<FormValidatorResponse> Check(string value, CancellationToken cancellationToken)
        {
            if (value.Length < 10)
            {
                return new FormValidatorResponse(false, "Password is not long enough");
            }

            CheckPasswordBreachReply reply;
            try
            {
                reply = await _authClient.CheckPasswordBreachAsync(
                    new CheckPasswordBreachRequest
                    {
                        Password = value,
                    },
                    null,
                    null,
                    cancellationToken
                );
            }
            catch
            {
                return new FormValidatorResponse(true, null);
            }

            if (reply.IsBreached)
            {
                return new FormValidatorResponse(false, "Password is not unique enough");
            }

            return new FormValidatorResponse(true, null);
        }
    }
}
