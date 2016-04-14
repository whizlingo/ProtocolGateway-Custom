using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Gateway.Tests;
using Microsoft.ServiceBus.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskExtensions = Microsoft.Azure.Devices.Gateway.Tests.Extensions.TaskExtensions;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Microsoft.Azure.Devices.ProtocolGateway.Tests.DeviceToCloud
{
    [TestClass]
    public class WhenSendingAUnsubscribeRequestWithIncorrectTopic : TestsBase, IDisposable
    {
        const string NotificationQoS2Content = "{\"test\": \"exactly once\"}";

        /// <summary>
        /// Shoulds the verify message delivery from device to cloud over qo s2.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public async Task ShouldVerifyIncorrectTopicBehaviour()
        {
            // Get the device from the registry.
            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(IotHubConnectionString);
            Device device = await registryManager.GetDeviceAsync(this.DeviceId);

            ServiceClient serviceClient = ServiceClient.CreateFromConnectionString(IotHubConnectionString);
            EventHubClient eventHubClient = EventHubClient.CreateFromConnectionString(IotHubConnectionString, "messages/events");
            EventHubConsumerGroup ehGroup = eventHubClient.GetConsumerGroup("test");

            string[] partitionIds = (await eventHubClient.GetRuntimeInformationAsync()).PartitionIds;
            EventHubReceiver[] receivers = await Task.WhenAll(partitionIds.Select(pid => ehGroup.CreateReceiverAsync(pid.Trim(), DateTime.UtcNow)));


            var testPromise = new TaskCompletionSource();

            // Setting  up the  TCP Client to send command to Protocol Gateway.
            var group = new MultithreadEventLoopGroup();
            string targetHost = this.TlsCertificate.GetNameInfo(X509NameType.DnsName, false);
            Bootstrap bootstrap = new Bootstrap()
                .Group(group)
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Handler(new ActionChannelInitializer<ISocketChannel>(ch => ch.Pipeline.AddLast(
                   TlsHandler.Client(targetHost, null, (sender, certificate, chain, errors) => true),
                    MqttEncoder.Instance,
                    new MqttDecoder(false, 256 * 1024),
                    new TestScenarioRunner(cmf => GetClientScenarioForUnsubscribeIncorrectTopic(cmf, this.DeviceId, device.Authentication.SymmetricKey.PrimaryKey, ClientId), testPromise, CommunicationTimeout, CommunicationTimeout),
                    new XUnitLoggingHandler(this.output))));
            IChannel clientChannel = await bootstrap.ConnectAsync(this.ServerAddress, this.ProtocolGatewayPort);
            this.ScheduleCleanup(async () =>
            {
                await clientChannel.CloseAsync();
                await group.ShutdownGracefullyAsync();
            });

            Task timeoutTask = Task.Run(async () =>
            {
                await Task.Delay(TestTimeout);
                throw new TimeoutException("Test has timed out.");
            });

            Task testWorkTask = Task.Run(async () =>
            {
                Tuple<EventData, string>[] ehMessages = await CollectEventHubMessagesAsync(receivers, 1);
                Assert.IsTrue(ehMessages.Select(x => NotificationQoS2Content.Equals(x.Item2, StringComparison.Ordinal)).Count() == 1);
            });

            await TaskExtensions.WhenSome(new[]
            {
                testPromise.Task,
                testWorkTask,
                timeoutTask
            }, 2);
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public async Task ShouldVerifyIncorrectVinBehaviour()
        {
            // Get the device from the registry.
            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(IotHubConnectionString);
            Device device = await registryManager.GetDeviceAsync(this.DeviceId);

            ServiceClient serviceClient = ServiceClient.CreateFromConnectionString(IotHubConnectionString);
            EventHubClient eventHubClient = EventHubClient.CreateFromConnectionString(IotHubConnectionString, "messages/events");
            EventHubConsumerGroup ehGroup = eventHubClient.GetConsumerGroup("test");

            string[] partitionIds = (await eventHubClient.GetRuntimeInformationAsync()).PartitionIds;
            EventHubReceiver[] receivers = await Task.WhenAll(partitionIds.Select(pid => ehGroup.CreateReceiverAsync(pid.Trim(), DateTime.UtcNow)));


            var testPromise = new TaskCompletionSource();

            // Setting  up the  TCP Client to send command to Protocol Gateway.
            var group = new MultithreadEventLoopGroup();
            string targetHost = this.TlsCertificate.GetNameInfo(X509NameType.DnsName, false);
            Bootstrap bootstrap = new Bootstrap()
                .Group(group)
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Handler(new ActionChannelInitializer<ISocketChannel>(ch => ch.Pipeline.AddLast(
                   TlsHandler.Client(targetHost, null, (sender, certificate, chain, errors) => true),
                    MqttEncoder.Instance,
                    new MqttDecoder(false, 256 * 1024),
                    new TestScenarioRunner(cmf => GetClientScenarioForUnsubscribeIncorrectVin(cmf, this.DeviceId, device.Authentication.SymmetricKey.PrimaryKey, ClientId), testPromise, CommunicationTimeout, CommunicationTimeout),
                    new XUnitLoggingHandler(this.output))));
            IChannel clientChannel = await bootstrap.ConnectAsync(this.ServerAddress, this.ProtocolGatewayPort);
            this.ScheduleCleanup(async () =>
            {
                await clientChannel.CloseAsync();
                await group.ShutdownGracefullyAsync();
            });

            Task timeoutTask = Task.Run(async () =>
            {
                await Task.Delay(TestTimeout);
                throw new TimeoutException("Test has timed out.");
            });

            Task testWorkTask = Task.Run(async () =>
            {
                Tuple<EventData, string>[] ehMessages = await CollectEventHubMessagesAsync(receivers, 1);
                Assert.IsTrue(ehMessages.Select(x => NotificationQoS2Content.Equals(x.Item2, StringComparison.Ordinal)).Count() == 1);
            });

            await TaskExtensions.WhenSome(new[]
            {
                testPromise.Task,
                testWorkTask,
                timeoutTask
            }, 2);
        }


        static IEnumerable<TestScenarioStep> GetClientScenarioForUnsubscribeIncorrectTopic(Func<object> currentMessageFunc, string deviceId, string password, string clientIdlocal)
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

            int subscribePacketId = GetRandomPacketId();
            yield return TestScenarioStep.Write(new SubscribePacket
            {
                PacketId = subscribePacketId,
                Requests = new[]
                {
                        new SubscriptionRequest(string.Format("/{0}/{1}/TCU_COMMAND","NA",clientIdlocal), QualityOfService.AtLeastOnce)
                }
            });

            var subAckPacket = Xunit.Assert.IsType<SubAckPacket>(currentMessageFunc());
            Xunit.Assert.Equal(subscribePacketId, subAckPacket.PacketId);
            Xunit.Assert.Equal(1, subAckPacket.ReturnCodes.Count);
            Xunit.Assert.Equal(QualityOfService.AtLeastOnce, subAckPacket.ReturnCodes[0]);
            int UnsubPacketId = GetRandomPacketId();

            yield return TestScenarioStep.Write(new UnsubscribePacket
            {
                PacketId = UnsubPacketId,
                TopicFilters = new[]
                {///NA/5LMCJ2A90FUJ00139/TCU_COMMAND
                    string.Format("/{0}/{1}/INCORRECT_Topic","NA",clientIdlocal)
                  // string.Format("/{0}/{1}/TCU_COMMAND","NA",clientIdlocal)
                }

            });

            var UnsubAckPacket = Xunit.Assert.IsType<UnsubAckPacket>(currentMessageFunc());
            Xunit.Assert.Equal(UnsubPacketId, UnsubAckPacket.PacketId);

        }
        static IEnumerable<TestScenarioStep> GetClientScenarioForUnsubscribeIncorrectVin(Func<object> currentMessageFunc, string deviceId, string password, string clientIdlocal)
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

            int subscribePacketId = GetRandomPacketId();
            yield return TestScenarioStep.Write(new SubscribePacket
            {
                PacketId = subscribePacketId,
                Requests = new[]
                {
                        new SubscriptionRequest(string.Format("/{0}/{1}/TCU_COMMAND","NA",clientIdlocal), QualityOfService.AtLeastOnce)
                }
            });

            var subAckPacket = Xunit.Assert.IsType<SubAckPacket>(currentMessageFunc());
            Xunit.Assert.Equal(subscribePacketId, subAckPacket.PacketId);
            Xunit.Assert.Equal(1, subAckPacket.ReturnCodes.Count);
            Xunit.Assert.Equal(QualityOfService.AtLeastOnce, subAckPacket.ReturnCodes[0]);
            int UnsubPacketId = GetRandomPacketId();
            yield return TestScenarioStep.Write(new UnsubscribePacket
            {
                PacketId = UnsubPacketId,
                TopicFilters = new[]
                {
                     string.Format("/{0}/{1}/TCU_COMMAND","NA",clientIdlocal)
                }

            });

            var UnsubAckPacket = Xunit.Assert.IsType<UnsubAckPacket>(currentMessageFunc());
            Xunit.Assert.Equal(UnsubPacketId, UnsubAckPacket.PacketId);

        }
    }
}
