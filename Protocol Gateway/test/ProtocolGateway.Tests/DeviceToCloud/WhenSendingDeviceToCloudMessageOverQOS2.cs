using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Net;
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
    public class WhenSendingDeviceToCloudMessageOverQos2 : TestsBase, IDisposable
    {
        private const string NotificationQoS2Content = "{\"test\": \"exactly once\"}";

        private int packetId;

        private static int _qos2PacketId;
        private static object _messageId;

        /// <summary>
        /// Shoulds the verify message delivery from device to cloud over qo s2.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task ShouldVerifyMessageDeliveryFromDeviceToCloudOverQos2()
        {
            string iotHubConnectionString = ConfigurationManager.AppSettings["IOTHubConnectionString"];
            IotHubConnectionStringBuilder iotConnectionStringBuilder =
                IotHubConnectionStringBuilder.Create(iotHubConnectionString);

            // Get the device from the registry.
            this.DeviceId = ConfigurationManager.AppSettings["End2End.DeviceName"];
            this.ClientId = ConfigurationManager.AppSettings["End2End.ClientId"];
            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(iotHubConnectionString);
            Device device = await registryManager.GetDeviceAsync(this.DeviceId);

            ServiceClient serviceClient = ServiceClient.CreateFromConnectionString(iotHubConnectionString);
            EventHubClient eventHubClient = EventHubClient.CreateFromConnectionString(iotHubConnectionString,
                "messages/events");
            EventHubConsumerGroup ehGroup = eventHubClient.GetConsumerGroup("test");

            string[] partitionIds = (await eventHubClient.GetRuntimeInformationAsync()).PartitionIds;
            EventHubReceiver[] receivers =
                await Task.WhenAll(partitionIds.Select(pid => ehGroup.CreateReceiverAsync(pid.Trim(), DateTime.UtcNow)));


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
                    new TestScenarioRunner(
                        cmf =>
                            GetClientScenarioForQos2WithIntermidiateStates(cmf, this.DeviceId,
                                device.Authentication.SymmetricKey.PrimaryKey, ClientId,DisconnectStep.Complete), testPromise,
                        CommunicationTimeout, CommunicationTimeout),
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
                Assert.IsTrue(
                    ehMessages.Select(x => NotificationQoS2Content.Equals(x.Item2, StringComparison.Ordinal)).Count() ==
                    1);
            });

            await TaskExtensions.WhenSome(new[]
            {
                testPromise.Task,
                testWorkTask,
                timeoutTask
            }, 2);

            // making sure that device queue is empty.
            await Task.Delay(TimeSpan.FromSeconds(3));

            DeviceClient deviceClient = DeviceClient.Create(
                iotConnectionStringBuilder.HostName,
                new DeviceAuthenticationWithRegistrySymmetricKey(this.DeviceId,
                    device.Authentication.SymmetricKey.PrimaryKey));
            Client.Message message = await deviceClient.ReceiveAsync(TimeSpan.FromSeconds(1));
            Assert.IsTrue(message == null);
        }

        [TestMethod]
        public async Task ShouldVerifyIfMessageIsRetriedInCaseTheDeviceDisconnectsAfterGettingPUBREC()
        {
            string iotHubConnectionString = ConfigurationManager.AppSettings["IOTHubConnectionString"];
            IotHubConnectionStringBuilder iotConnectionStringBuilder =
                IotHubConnectionStringBuilder.Create(iotHubConnectionString);

            // Get the device from the registry.
            this.DeviceId = ConfigurationManager.AppSettings["End2End.DeviceName"];
            this.ClientId = ConfigurationManager.AppSettings["End2End.ClientId"];
            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(iotHubConnectionString);
            Device device = await registryManager.GetDeviceAsync(this.DeviceId);

            ServiceClient serviceClient = ServiceClient.CreateFromConnectionString(iotHubConnectionString);
            EventHubClient eventHubClient = EventHubClient.CreateFromConnectionString(iotHubConnectionString,
                "messages/events");
            EventHubConsumerGroup ehGroup = eventHubClient.GetConsumerGroup("test");

            string[] partitionIds = (await eventHubClient.GetRuntimeInformationAsync()).PartitionIds;
            EventHubReceiver[] receivers =
                await Task.WhenAll(partitionIds.Select(pid => ehGroup.CreateReceiverAsync(pid.Trim(), DateTime.UtcNow)));


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
                    new TestScenarioRunner(
                        cmf =>
                            GetClientScenarioForQos2WithIntermidiateStates(cmf, this.DeviceId,
                                device.Authentication.SymmetricKey.PrimaryKey, ClientId,DisconnectStep.BeforePubRel), testPromise,
                        CommunicationTimeout, CommunicationTimeout),
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
                Assert.IsTrue(
                    ehMessages.Select(x => NotificationQoS2Content.Equals(x.Item2, StringComparison.Ordinal)).Count() ==
                    1);
            });

            await TaskExtensions.WhenSome(new[]
            {
                testPromise.Task,
                testWorkTask,
                timeoutTask
            }, 2);

            // Connecting the client again.
            var groupforSecondTimeConnection = new MultithreadEventLoopGroup();
            string targetHostForSecondTimeConnection = this.TlsCertificate.GetNameInfo(X509NameType.DnsName, false);
            Bootstrap bootstrapForSecondTimeConnection = new Bootstrap()
                .Group(groupforSecondTimeConnection)
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Handler(new ActionChannelInitializer<ISocketChannel>(ch => ch.Pipeline.AddLast(
                    TlsHandler.Client(targetHostForSecondTimeConnection, null,
                        (sender, certificate, chain, errors) => true),
                    MqttEncoder.Instance,
                    new MqttDecoder(false, 256 * 1024),
                    new TestScenarioRunner(
                        cmf =>
                            GetClientScenarioForResumingTheQOS2PublishAgainAfterConnectAndComplete(cmf, this.DeviceId,
                                device.Authentication.SymmetricKey.PrimaryKey, this.ClientId),
                        testPromise,
                        CommunicationTimeout,
                        CommunicationTimeout),
                    new XUnitLoggingHandler(this.output))));
            IChannel clientChannelForSecondTimeConnection =
                await bootstrapForSecondTimeConnection.ConnectAsync(this.ServerAddress, this.ProtocolGatewayPort);
            this.ScheduleCleanup(async () =>
            {
                await clientChannelForSecondTimeConnection.CloseAsync();
                await groupforSecondTimeConnection.ShutdownGracefullyAsync();
            });


            Task testWorkTaskForSecondExecution = Task.Run(async () => { });

            Task timeoutTaskForSecondTimeExecution = Task.Run(async () =>
            {
                await Task.Delay(TestTimeout);
                throw new TimeoutException("Test has timed out.");
            });

            await TaskExtensions.WhenSome(new[]
            {
                testPromise.Task,
                testWorkTaskForSecondExecution,
                timeoutTaskForSecondTimeExecution
            }, 2);

            // making sure that device queue is empty.
            await Task.Delay(TimeSpan.FromSeconds(3));

            DeviceClient deviceClient = DeviceClient.Create(
                iotConnectionStringBuilder.HostName,
                new DeviceAuthenticationWithRegistrySymmetricKey(this.DeviceId,
                    device.Authentication.SymmetricKey.PrimaryKey));
            Client.Message message = await deviceClient.ReceiveAsync(TimeSpan.FromSeconds(1));
            Assert.IsTrue(message == null, string.Format("Message is not null: {0}", message));
        }

        public enum DisconnectStep
        {
            BeforePubRel,
            AfterPubcomp,
            Complete
        }

        private static IEnumerable<TestScenarioStep> GetClientScenarioForQos2WithIntermidiateStates(
            Func<object> currentMessageFunc,
            string deviceId,
            string password,
            string clientIdlocal,
            DisconnectStep disconnectStep)
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
                    new SubscriptionRequest(string.Format("/{0}/{1}/TCU_COMMAND", "NA", deviceId),
                        QualityOfService.ExactlyOnce)
                }
            });

            var subAckPacket = Xunit.Assert.IsType<SubAckPacket>(currentMessageFunc());
            Xunit.Assert.Equal(subscribePacketId, subAckPacket.PacketId);
            Xunit.Assert.Equal(1, subAckPacket.ReturnCodes.Count);
            Xunit.Assert.Equal(QualityOfService.ExactlyOnce, subAckPacket.ReturnCodes[0]);

            _qos2PacketId = GetRandomPacketId();
            var publishPacket = new PublishPacket(QualityOfService.ExactlyOnce, false, false)
            {
                PacketId = _qos2PacketId,
                TopicName = string.Format("{0}/{1}/TCU_ALERT", "NA", deviceId),
                Payload = Unpooled.WrappedBuffer(Encoding.UTF8.GetBytes(NotificationQoS2Content))
            };

            yield return TestScenarioStep.Write(
              publishPacket);
            var packets = new Packet[1];
            for (int i = packets.Length - 1; i >= 0; i--)
            {
                packets[i] = Xunit.Assert.IsAssignableFrom<Packet>(currentMessageFunc());
                if (i > 0)
                {
                    yield return TestScenarioStep.ReadMore();
                }
            }

            Assert.IsTrue(
                packets.OfType<PubRecPacket>().Count(x => x.QualityOfService == QualityOfService.AtMostOnce) != 0);

            var firstPubRec =
                packets.OfType<PubRecPacket>().FirstOrDefault(x => x.QualityOfService == QualityOfService.AtMostOnce);

            if (disconnectStep == DisconnectStep.BeforePubRel)
            {
                yield return TestScenarioStep.Write(false, DisconnectPacket.Instance);
            }
            else
            {
                yield return TestScenarioStep.Write(PubRelPacket.InResponseTo(firstPubRec));
                var pubCompPacket = Xunit.Assert.IsAssignableFrom<PubCompPacket>(currentMessageFunc());
                Xunit.Assert.Equal(firstPubRec.PacketId, pubCompPacket.PacketId);
                yield return TestScenarioStep.Write(false, DisconnectPacket.Instance);
            }
        }

        private static IEnumerable<TestScenarioStep>
            GetClientScenarioForResumingTheQOS2PublishAgainAfterConnectAndComplete(
            Func<object> currentMessageFunc,
            string deviceId,
            string password,
            string clientId)
        {
            // Connect with clean session set to false.
            yield return TestScenarioStep.Write(new ConnectPacket
            {
                ClientId = clientId,
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

            var connAckPacketSecondtime = Xunit.Assert.IsType<ConnAckPacket>(currentMessageFunc());
            Xunit.Assert.Equal(ConnectReturnCode.Accepted, connAckPacketSecondtime.ReturnCode);

            int subscribePacketIdSecondTime = GetRandomPacketId();
            yield return TestScenarioStep.Write(new SubscribePacket
            {
                PacketId = subscribePacketIdSecondTime,
                Requests = new[]
                {
                    new SubscriptionRequest(string.Format("/{0}/{1}/TCU_COMMAND", "NA", clientId),
                        QualityOfService.ExactlyOnce)
                }
            });

            yield return TestScenarioStep.Write(
                new PublishPacket(QualityOfService.ExactlyOnce, false, false)
                {
                    PacketId = _qos2PacketId,
                    TopicName = string.Format("{0}/{1}/TCU_ALERT", "NA", deviceId),
                    Payload = Unpooled.WrappedBuffer(Encoding.UTF8.GetBytes(NotificationQoS2Content))
                });


            var packets = new Packet[1];
            for (int i = packets.Length - 1; i >= 0; i--)
            {
                packets[i] = Xunit.Assert.IsAssignableFrom<Packet>(currentMessageFunc());
                if (i > 0)
                {
                    yield return TestScenarioStep.ReadMore();
                }
            }

            Assert.IsTrue(
                packets.OfType<PubRecPacket>().Count(x => x.QualityOfService == QualityOfService.AtMostOnce) != 0);

            var firstPubRec =
                packets.OfType<PubRecPacket>().FirstOrDefault(x => x.QualityOfService == QualityOfService.AtMostOnce);
            
            yield return TestScenarioStep.Write(PubRelPacket.InResponseTo(firstPubRec));
            var pubCompPacket = Xunit.Assert.IsAssignableFrom<PubCompPacket>(currentMessageFunc());
            Xunit.Assert.Equal(firstPubRec.PacketId, pubCompPacket.PacketId);
            yield return TestScenarioStep.Write(false, DisconnectPacket.Instance);

        }
    }
}
