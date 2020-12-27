using System.Diagnostics;
using System.Threading;
using Xunit;

namespace AuthServer.Server.Tests.Visual
{
    public class PercyTest
    {
        [Fact]
        public void TestValidHash()
        {
            Thread.Sleep(1000);

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                AuthServer.Server.Program.Main(new string[0]);
            }).Start();


            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "/usr/bin/npx",
                Arguments = "percy exec -- node snapshot.js",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };

            Process p = new Process { StartInfo = startInfo };
            p.Start();
            p.WaitForExit();

            string error = p.StandardError.ReadToEnd();
            Assert.Empty(error);
        }
    }
}
