using System;
using System.Net.Mail;
using System.Threading.Tasks;

namespace AuthServer.Client.Shared.Components.Form.FormValidator
{
    public class EmailValidator : IFormValidator
    {
        public Task<FormValidatorResponse> Check(string value)
        {
            try {
                MailAddress addr = new MailAddress(value);
                if (addr.Address == value)
                {
                    return Task.FromResult(new FormValidatorResponse(true, null));
                }
            } catch {}

            return Task.FromResult(new FormValidatorResponse(false, "Email is not valid"));
        }
    }
}
