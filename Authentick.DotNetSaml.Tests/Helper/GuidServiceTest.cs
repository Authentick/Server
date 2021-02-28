using System;
using Authentick.DotNetSaml.Helper;
using Xunit;

namespace Authentick.DotNetSaml.Tests.Helper
{
    public class GuidServiceTest
    {
        [Fact]
        public void TestNewGuid()
        {
            GuidService service = new GuidService();
            Assert.IsType<Guid>(service.NewGuid());
        }
    }
}
