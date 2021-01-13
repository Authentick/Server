using System;
using System.Collections.Generic;
using Gatekeeper.Client.Pages.Admin.Apps.Setup.Steps;
using AuthServer.Shared.Admin;

namespace Gatekeeper.Client.Pages.Admin.Apps.Setup
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
            _currentStep = new TypeSelectionStep();
        }

        public AddNewAppRequest GetAddNewAppRequest()
        {
            return _addNewAppRequest;
        }

        public void FinishStep(IStep step)
        {
            switch (step)
            {
                case TypeSelectionStep typeSelectionStep:
                    switch (typeSelectionStep.Type)
                    {
                        case "Other":
                            _addNewAppRequest.HostingType = HostingType.NonWeb;
                            break;
                        case "Web":
                            break;
                        default:
                            throw new NotImplementedException(typeSelectionStep.Type + " is not implemented");
                    }
                    break;
                case WebHostingTypeSelectionStep webHostingTypeSelectionStep:
                    switch (webHostingTypeSelectionStep.Type)
                    {
                        case "Cloud":
                            _addNewAppRequest.HostingType = HostingType.WebGeneric;
                            break;
                        case "Self-Hosted":
                            break;
                        default:
                            throw new NotImplementedException(webHostingTypeSelectionStep.Type + " is not implemented");
                    }
                    break;
                case SelfHostedAccessSelectionStep selfHostedAccessSelectionStep:
                    switch (selfHostedAccessSelectionStep.Type)
                    {
                        case "Gatekeeper Proxy":
                            _addNewAppRequest.HostingType = HostingType.WebGatekeeperProxy;
                            break;
                        case "Directly":
                            _addNewAppRequest.HostingType = HostingType.WebGeneric;
                            break;
                        default:
                            throw new NotImplementedException(selfHostedAccessSelectionStep.Type + " is not implemented");
                    }
                    break;
                case AppInformationStep appInformationStep:
                    _addNewAppRequest.Name = appInformationStep.Name;
                    _addNewAppRequest.Description = appInformationStep.Description;
                    if (_addNewAppRequest.HostingType != HostingType.NonWeb) 
                    {
                        _addNewAppRequest.Url = appInformationStep.Url;
                    }

                    if(_addNewAppRequest.HostingType == HostingType.WebGatekeeperProxy) 
                    {
                        _addNewAppRequest.ProxySetting = new AddNewAppRequest.Types.ProxySetting
                        {
                            InternalHostname = appInformationStep.InternalUrl,
                            PublicHostname = appInformationStep.PublicDomain,
                        };              
                    }
                    
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
                case ConfigureOpenIDConnectStep configureOpenIDConnectStep:
                    _addNewAppRequest.OidcSetting = new AddNewAppRequest.Types.OIDCSetting
                    {
                        RedirectUri = configureOpenIDConnectStep.RedirectUrl,
                    };
                    break;
                case ConfigureScimStep configureScimStep:
                    _addNewAppRequest.ScimSetting = new AddNewAppRequest.Types.SCIMSetting
                    {
                        Endpoint = configureScimStep.scimSettings.Endpoint,
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
