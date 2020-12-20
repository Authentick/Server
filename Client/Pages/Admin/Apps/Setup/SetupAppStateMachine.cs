using System;
using System.Collections.Generic;
using AuthServer.Client.Pages.Admin.Apps.Setup.Steps;
using AuthServer.Shared.Admin;

namespace AuthServer.Client.Pages.Admin.Apps.Setup
{
    public class SetupAppStateMachine
    {
        public event Action? OnChange;
        IStep? _currentStep;
        private Stack<IStep> PreviousSteps = new Stack<IStep>();
        private IStep? NextStep;
        private AddNewAppRequest _addNewAppRequest = new AddNewAppRequest();

        internal void Initialize()
        {
            _currentStep = new InitialStep();
        }

        internal AddNewAppRequest GetAddNewAppRequest()
        {
            return _addNewAppRequest;
        }

        public void FinishStep(IStep step)
        {
            switch (step)
            {
                case InitialStep initialStep:
                    _addNewAppRequest.Name = initialStep.Name;
                    break;
                case ChooseAuthMethodStep chooseAuthMethodStep:
                    _addNewAppRequest.AuthChoice = chooseAuthMethodStep.AuthChoice;
                    break;
                case ChooseDirectoryMethodStep chooseDirectoryMethodStep:
                    _addNewAppRequest.DirectoryChoice = chooseDirectoryMethodStep.DirectoryChoice;
                    break;
                case ConfigureAccessGroupsStep configureAccessGroupsStep:
                    _addNewAppRequest.GroupIds.AddRange(configureAccessGroupsStep.SelectedGroups);
                    break;
                case ConfigureGatekeeperProxyStep configureGatekeeperProxyStep:
                    _addNewAppRequest.ProxySetting = new AddNewAppRequest.Types.ProxySetting
                    {
                        InternalHostname = configureGatekeeperProxyStep.gatekeeperProxySettings.InternalDomainName,
                        PublicHostname = configureGatekeeperProxyStep.gatekeeperProxySettings.PublicDomainName,
                    };
                    break;
                case ConfigureOpenIDConnectStep configureOpenIDConnectStep:
                    _addNewAppRequest.OidcSetting = new AddNewAppRequest.Types.OIDCSetting
                    {
                        RedirectUri = configureOpenIDConnectStep.RedirectUrl,
                    };
                    break;
                case ConfigureScimStep configureScimStep:
                    _addNewAppRequest.ScimSetting = new AddNewAppRequest.Types.SCIMSetting
                    {
                        Hostname = configureScimStep.scimSettings.BaseDomain,
                        Credentials = configureScimStep.scimSettings.Credentials,
                    };
                    break;
            }

            PreviousSteps.Push(step);
            _currentStep = NextStep;
            NextStep = null;
        }

        public void GoBack(IStep step)
        {
            _currentStep = PreviousSteps.Pop();
        }

        public void SetNextStep(IStep? step)
        {
            NextStep = step;
            OnChange?.Invoke();
        }

        public bool HasNextStep()
        {
            return (NextStep != null);
        }

        public bool HasPreviousStep()
        {
            return PreviousSteps.Count > 0;
        }

        public IStep GetCurrentStep()
        {
            return _currentStep;
        }
    }
}
