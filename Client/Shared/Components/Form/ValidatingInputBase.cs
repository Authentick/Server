using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuthServer.Client.Shared.Components.Form.FormValidator;
using Microsoft.AspNetCore.Components;

namespace AuthServer.Client.Shared.Components.Form
{
    public class ValidatingInputBase : ComponentBase
    {
        [Parameter]
        public List<IFormValidator> FormValidators { get; set; } = null!;
        [Parameter]
        public string Value { get; set; } = null!;
        [Parameter]
        public EventCallback<string> ValueChanged { get; set; }
        [Parameter]
        public string Name { get; set; } = null!;
        [Parameter]
        public string Placeholder { get; set; } = null!;

        protected ValidationStateEnum _validationState { get; set; }
        protected string? _errorHint { get; set; }
        protected Guid _divIdentifier = Guid.NewGuid();

        protected async Task KeyPressed()
        {
            _validationState = ValidationStateEnum.Checking;
            await ValueChanged.InvokeAsync(Value);

            foreach (FormValidator.IFormValidator validator in FormValidators)
            {
                FormValidator.FormValidatorResponse reply = await validator.Check(Value);
                if (!reply.IsValid)
                {
                    _validationState = ValidationStateEnum.Failed;
                    _errorHint = reply.ErrorMessage;
                    return;
                }
            }

            _validationState = ValidationStateEnum.Success;
        }

        protected string GetWrapperClasses()
        {
            string cssClasses = GetCssClasses();

            if (cssClasses == "")
            {
                return "";
            }

            return "wrapper-" + GetCssClasses();
        }
        protected string GetCssClasses()
        {
            switch (_validationState)
            {
                case ValidationStateEnum.Checking:
                    return "checking";
                case ValidationStateEnum.Failed:
                    return "failed";
                case ValidationStateEnum.Success:
                    return "success";
            }

            return "";
        }

        protected enum ValidationStateEnum
        {
            None = 0,
            Success = 1,
            Failed = 2,
            Checking = 3,
        }
    }
}
