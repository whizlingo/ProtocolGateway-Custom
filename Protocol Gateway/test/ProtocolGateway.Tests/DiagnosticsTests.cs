// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Devices.ProtocolGateway.Tests
{
    using global::ProtocolGateway.Samples.Common;
    using Microsoft.Azure.Devices.ProtocolGateway.Instrumentation;
    using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Utility;
    using Xunit;

    public class DiagnosticsTests
    {
        [Fact]
        public void VerifyEventSources()
        {
            EventSourceAnalyzer.InspectAll(MqttIotHubAdapterEventSource.Log);
            EventSourceAnalyzer.InspectAll(BootstrapperEventSource.Log);
        }
    }
}