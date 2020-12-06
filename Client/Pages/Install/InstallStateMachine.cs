using System;
using AuthServer.Client.Pages.Install.Steps;

namespace AuthServer.Client.Pages.Install
{
    public class InstallStateMachine
    {
        public event Action OnChange;

        IStep _currentStep;
        private IStep? PreviousStep;
        private IStep? NextStep;

        internal void Initialize()
        {
            _currentStep = new InitialSetupStep();
        }

        public void FinishStep(IStep step)
        {
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
