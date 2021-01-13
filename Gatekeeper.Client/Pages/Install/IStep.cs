using Microsoft.AspNetCore.Components;

namespace Gatekeeper.Client.Pages.Install
{
    public interface IStep
    {
        [Parameter]
        InstallStateMachine StateMachine { get; set; }
    }
}
