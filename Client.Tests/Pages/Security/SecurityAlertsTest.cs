using Xunit;
using Bunit;
using AuthServer.Client.Pages.Security;

namespace AuthServer.Client.Tests.Pages.Security
{
    public class SecurityAlertsTest
    {
        [Fact]
        public void ComponentRendersCorrectly()
        {
            using var ctx = new TestContext();

            var cut = ctx.RenderComponent<SecurityAlerts>();

            cut.MarkupMatches("");
        }
    }
}
