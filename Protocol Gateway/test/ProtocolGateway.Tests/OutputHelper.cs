using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit.Abstractions;

namespace Microsoft.Azure.Devices.ProtocolGateway.Tests
{
    public class OutputHelper : ITestOutputHelper
    {
        private readonly TestContext testContext;

        public OutputHelper(TestContext testContext)
        {
            this.testContext = testContext;
        }

        public void WriteLine(string message)
        {
            this.testContext.WriteLine(message);
        }

        public void WriteLine(string format, params object[] args)
        {
            this.testContext.WriteLine(format, args);
        }
    }
}