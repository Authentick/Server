using System;
using Gatekeeper.Client.Pages.Install.Steps;
using AuthServer.Shared;

namespace Gatekeeper.Client.Pages.Install
{
    public class InstallStateMachine
    {
        public event Action OnChange;

        IStep _currentStep;
        private IStep? PreviousStep;
        private IStep? NextStep;
        private SetupInstanceRequest _setupInstanceRequest = new SetupInstanceRequest();
        public string? AuthToken;
        public string? DomainName;
        public string? AcmeContactEmailAddress;

        public void Initialize()
        {
            _currentStep = new InitialSetupStep();
        }

        public void ResumeFromHttps()
        {
            _currentStep = new EmailSelectionStep();
        }

        internal SetupInstanceRequest GetSetupInstanceRequest()
        {
            return _setupInstanceRequest;
        }

        public void FinishStep(IStep step)
        {
            switch (step)
            {
                case EmailCustomSettingsStep emailStep:
                    _setupInstanceRequest.SmtpSettings = new SetupSmtpData
                    {
                        Hostname = emailStep.emailSettings.Hostname,
                        Password = emailStep.emailSettings.Password,
                        SenderAddress = emailStep.emailSettings.SenderAddress,
                        Username = emailStep.emailSettings.Username,
                        Port = Int32.Parse(emailStep.emailSettings.Port),
                    };
                    break;
                case ConfigureLetsEncryptCertificateStep tlsStep:
                    DomainName = tlsStep.letsEncryptCertificateSettings.DomainName;
                    AcmeContactEmailAddress = tlsStep.letsEncryptCertificateSettings.Email;
                    break;
                case AccountCreationStep accountCreationStep:
                    _setupInstanceRequest.AccountData = new SetupAccountData
                    {
                        Username = accountCreationStep.userAccount.Username,
                        Email = accountCreationStep.userAccount.Email,
                        Password = accountCreationStep.userAccount.Password,
                    };
                    return;
            }

            _currentStep = NextStep;
        }

        public void GoBack(IStep step)
        {
            _currentStep = PreviousStep;
        }

        public void SetPreviousStep(IStep? step)
        {
            PreviousStep = step;
            OnChange?.Invoke();
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
            return (PreviousStep != null);
        }

        public IStep GetCurrentStep()
        {
            return _currentStep;
        }
    }
}
