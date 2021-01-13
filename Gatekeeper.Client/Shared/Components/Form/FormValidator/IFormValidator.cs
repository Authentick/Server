using System.Threading;
using System.Threading.Tasks;

namespace Gatekeeper.Client.Shared.Components.Form.FormValidator
{
    public interface IFormValidator
    {
        public Task<FormValidatorResponse> Check(string value, CancellationToken cancellationToken);
    }

    public class FormValidatorResponse
    {
        public readonly bool IsValid;
        public readonly string? ErrorMessage;

        public FormValidatorResponse(bool isValid, string? errorMessage)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
        }
    }
}
