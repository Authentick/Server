using Xunit;
using Bunit;
using Gatekeeper.Client.Pages.Security;

namespace Gatekeeper.Client.Tests.Pages.Security
{
    public class SecurityAlertsTest
    {
        [Fact]
        public void ComponentRendersCorrectly()
        {
            using var ctx = new TestContext();

            var cut = ctx.RenderComponent<SecurityAlerts>();

            string expected = @"<div>
  <h4 class=""font-weight-bold"">Action required</h4>
  <div class=""alert-card alert-card__red p-3 mb-4"">
    <p class=""font-weight-bold"">Login with failed Two-factor Authentication</p>
    <p>We have identified valid login attempts with failed two-factor authentication. This could indicate someone got access to your password.</p>
    <p class=""font-weight-bold"">Technical details</p>
    <p>Source IPS: X.X.X.X and 12 more</p>
    <p>Time: 12 minutes ago</p>
    <p class=""font-weight-bold"">Recommendation</p>
    <p>In case this wasn’t you, make sure to change your password. We have also informed your administrator about this issue.</p>
    <div class=""d-flex flex-column flex-md-row"">
      <a class=""btn btn-secondary-dark btn-sm-100 mr-0 mr-md-4 mb-2 mb-md-0"" href=""#"">Change my password</a>
      <a class=""btn btn-secondary-dark btn-sm-100"" href=""#"">This was me. Dismiss alert.</a>
    </div>
  </div>
  <h4 class=""font-weight-bold"">Automatically mitigated</h4>
  <div class=""alert-card alert-card__green p-3"">
    <p class=""font-weight-bold"">Bruteforce attempt</p>
    <p>We have detected a bruteforce attempt and have blocked the attacker.</p>
    <p class=""font-weight-bold"">Technical details</p>
    <p>Source IPS: X.X.X.X and 12 more</p>
    <p>Time: 12 minutes ago</p>
    <p class=""font-weight-bold"">Recommendation</p>
    <p>No action is required. The issue has been automatically mitigated.</p>
    <a class=""btn btn-secondary-dark btn-sm-100"" href=""#"">Load more</a>
  </div>
</div>";
            cut.MarkupMatches(expected);
        }
    }
}
