using System;

namespace Authentick.DotNetSaml.Helper
{
    internal class GuidService : IGuidService
    {
        public Guid NewGuid()
        {
            return Guid.NewGuid();
        }
    }
}
