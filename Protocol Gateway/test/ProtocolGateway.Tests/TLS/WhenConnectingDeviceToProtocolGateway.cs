using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Codecs.Mqtt;
using DotNetty.Codecs.Mqtt.Packets;
using DotNetty.Common.Concurrency;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Azure.Devices.Gateway.Tests;
using Microsoft.ServiceBus.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskExtensions = Microsoft.Azure.Devices.Gateway.Tests.Extensions.TaskExtensions;

namespace Microsoft.Azure.Devices.ProtocolGateway.Tests.TLS
{
    [TestClass]
    public class WhenConnectingDeviceToProtocolGateway : TestsBase
    {
        [TestMethod]
        public async Task ShouldConnectWithTls1WhenClientConnectsTls1()
        {
            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(IotHubConnectionString);
            Device device = await registryManager.GetDeviceAsync(this.DeviceId);

            // Setting  up the  TCP Client to send command to Protocol Gateway.
            var group = new MultithreadEventLoopGroup();
            string targetHost = this.TlsCertificate.GetNameInfo(X509NameType.DnsName, false);
            var testPromise = new TaskCompletionSource();
            var clientChannel = await GetClientChannelBySettingUpBoostrap(@group, targetHost, device, testPromise, SslProtocols.Tls);
            this.ScheduleCleanup(async () =>
            {
                await clientChannel.CloseAsync();
                await group.ShutdownGracefullyAsync();
            });
            Task testWorkTask = Task.Run(async () => { });

            await TaskExtensions.WhenSome(new[]
            {
                testPromise.Task,
                testWorkTask
            }, 2);
        }

        [TestMethod]
        public async Task ShouldConnectWithTls11WhenClientConnectsTls11()
        {
            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(IotHubConnectionString);
            Device device = await registryManager.GetDeviceAsync(this.DeviceId);

            // Setting  up the  TCP Client to send command to Protocol Gateway.
            var group = new MultithreadEventLoopGroup();
            string targetHost = this.TlsCertificate.GetNameInfo(X509NameType.DnsName, false);
            var testPromise = new TaskCompletionSource();
            var clientChannel = await GetClientChannelBySettingUpBoostrap(@group, targetHost, device, testPromise, SslProtocols.Tls11);
            this.ScheduleCleanup(async () =>
            {
                await clientChannel.CloseAsync();
                await group.ShutdownGracefullyAsync();
            });
            Task testWorkTask = Task.Run(async () => { });

            await TaskExtensions.WhenSome(new[]
            {
                testPromise.Task,
                testWorkTask
            }, 2);
        }

        [TestMethod]
        public async Task ShouldConnectWithTls12WhenClientConnectsTls12()
        {
            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(IotHubConnectionString);
            Device device = await registryManager.GetDeviceAsync(this.DeviceId);

            // Setting  up the  TCP Client to send command to Protocol Gateway.
            var group = new MultithreadEventLoopGroup();
            string targetHost = this.TlsCertificate.GetNameInfo(X509NameType.DnsName, false);
            var testPromise = new TaskCompletionSource();
            var clientChannel = await GetClientChannelBySettingUpBoostrap(@group, targetHost, device, testPromise, SslProtocols.Tls12);
            this.ScheduleCleanup(async () =>
            {
                await clientChannel.CloseAsync();
                await group.ShutdownGracefullyAsync();
            });
            Task testWorkTask = Task.Run(async () => { });

            await TaskExtensions.WhenSome(new[]
            {
                testPromise.Task,
                testWorkTask
            }, 2);
        }

        private async Task<IChannel> GetClientChannelBySettingUpBoostrap(
            MultithreadEventLoopGroup @group,
            string targetHost,
            Device device,
            TaskCompletionSource testPromise,
            SslProtocols sslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12)
        {
            Bootstrap bootstrap = new Bootstrap()
                .Group(@group)
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Handler(new ActionChannelInitializer<ISocketChannel>(ch => ch.Pipeline.AddLast(
                    TlsHandler.Client(targetHost, null, (sender, certificate, chain, errors) => true),
                    MqttEncoder.Instance,
                    new MqttDecoder(false, 256 * 1024),
                    new TestScenarioRunner(
                        cmf =>
                            GetClientScenarioForConnect(cmf, this.DeviceId,
                                device.Authentication.SymmetricKey.PrimaryKey, ClientId), testPromise,
                        CommunicationTimeout, CommunicationTimeout),
                    new XUnitLoggingHandler(this.output))));
            IChannel clientChannel = await bootstrap.ConnectAsync(this.ServerAddress, this.ProtocolGatewayPort);
            return clientChannel;
        }

        static IEnumerable<TestScenarioStep> GetClientScenarioForConnect(Func<object> currentMessageFunc, string deviceId, string password, string clientIdlocal)
        {
            // Connect with clean session set to false.
            yield return TestScenarioStep.Write(new ConnectPacket
            {
                ClientId = clientIdlocal,
                CleanSession = false,
                HasUsername = true,
                Username = deviceId,
                HasPassword = true,
                Password = password,
                KeepAliveInSeconds = 120,
                HasWill = true,
                WillTopicName = "last/word",
                WillMessage = Unpooled.WrappedBuffer(Encoding.UTF8.GetBytes("oops"))
            });

            var connAckPacket = Xunit.Assert.IsType<ConnAckPacket>(currentMessageFunc());
            Xunit.Assert.Equal(ConnectReturnCode.Accepted, connAckPacket.ReturnCode);

        }
    }
}
