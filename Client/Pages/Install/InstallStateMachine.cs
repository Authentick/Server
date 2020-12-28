using System;
using AuthServer.Client.Pages.Install.Steps;
using AuthServer.Shared;

namespace AuthServer.Client.Pages.Install
{
    public class InstallStateMachine
    {
        public event Action OnChange;

        IStep _currentStep;
        private IStep? PreviousStep;
        private IStep? NextStep;
        private SetupInstanceRequest _setupInstanceRequest = new SetupInstanceRequest();

        internal void Initialize()
        {
            _currentStep = new InitialSetupStep();
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
                    _setupInstanceRequest.TlsData = new SetupTlsData
                    {
                        Domain = tlsStep.letsEncryptCertificateSettings.DomainName,
                        ContactEmail = tlsStep.letsEncryptCertificateSettings.Email,
                    };

                    _setupInstanceRequest.PrimaryDomain = tlsStep.letsEncryptCertificateSettings.DomainName;
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
