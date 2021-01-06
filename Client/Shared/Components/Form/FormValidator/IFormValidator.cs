using System.Threading.Tasks;

namespace AuthServer.Client.Shared.Components.Form.FormValidator
{
    public interface IFormValidator
    {
        public Task<FormValidatorResponse> Check(string value);
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
