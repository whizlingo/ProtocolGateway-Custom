using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
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

namespace Microsoft.Azure.Devices.ProtocolGateway.Tests.CloudToDevice
{
    [TestClass]
    public class WhenSendingMessagesFromCloudToDeviceOnTwoSubscriptions : TestsBase
    {   
        [TestMethod]
        public async Task ShouldSuccessfullySendTwoCommandsAtTwoDifferentSubscriptionsForSameDevice()
        {
            IotHubConnectionStringBuilder hubConnectionStringBuilder = IotHubConnectionStringBuilder.Create(this.IotHubConnectionString);
            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(this.IotHubConnectionString);
            Device device = await registryManager.GetDeviceAsync(this.DeviceId);

            EventHubClient eventHubClient = EventHubClient.CreateFromConnectionString(this.IotHubConnectionString, "messages/events");
            EventHubConsumerGroup ehGroup = eventHubClient.GetDefaultConsumerGroup();

            string[] partitionIds = (await eventHubClient.GetRuntimeInformationAsync()).PartitionIds;
            EventHubReceiver[] receivers = await Task.WhenAll(partitionIds.Select(pid => ehGroup.CreateReceiverAsync(pid.Trim(), DateTime.UtcNow)));

            ServiceClient serviceClient = ServiceClient.CreateFromConnectionString(this.IotHubConnectionString);

            Stopwatch sw = Stopwatch.StartNew();

            var testPromise = new TaskCompletionSource();

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
                    new TestScenarioRunner(cmf => GetScenarioForCloudToDeviceQOS2(cmf,
                    this.DeviceId, device.Authentication.SymmetricKey.PrimaryKey, this.ClientId),
                    testPromise,
                    CommunicationTimeout,
                    CommunicationTimeout),
                    new XUnitLoggingHandler(this.output))));
            IChannel clientChannel = await bootstrap.ConnectAsync(this.ServerAddress, this.ProtocolGatewayPort);

            this.ScheduleCleanup(async () =>
            {
                await clientChannel.CloseAsync();
                await group.ShutdownGracefullyAsync();
            });

            Task testWorkTask = Task.Run(async () =>
            {
                Tuple<EventData, string>[] ehMessages = await CollectEventHubMessagesAsync(receivers, 2);

                Assert.IsTrue(ehMessages.Where(x => TelemetryQoS0Content.Equals(x.Item2, StringComparison.Ordinal)).Count() == 1);

                Assert.IsTrue(ehMessages.Where(x => TelemetryQoS1Content.Equals(x.Item2, StringComparison.Ordinal)).Count() == 1);

                string qosPropertyName = ConfigurationManager.AppSettings["QoSPropertyName"];
                var qos2Notification = new Message(Encoding.UTF8.GetBytes(NotificationQoS2Content));
                qos2Notification.Properties[qosPropertyName] = "2";
                qos2Notification.Properties["subTopic"] = "critical-alert";
                qos2Notification.Properties["region"] = "NA";
                qos2Notification.MessageId = Guid.NewGuid().ToString();
                qos2Notification.Properties["vin"] = this.ClientId;
                qos2Notification.Properties["TopicName"] = "TCU_COMMAND";
                await serviceClient.SendAsync(this.DeviceId, qos2Notification);
                
                var qos1Notification = new Message(Encoding.UTF8.GetBytes(NotificationQoS1Content));
                qos1Notification.Properties[qosPropertyName] = "1";
                qos1Notification.Properties["subTopic"] = "critical-alert";
                qos1Notification.Properties["region"] = "NA";
                qos1Notification.MessageId = Guid.NewGuid().ToString();
                qos1Notification.Properties["vin"] = this.ClientId;
                qos1Notification.Properties["TopicName"] = "SECURITY/SERVER_ERROR";
                await serviceClient.SendAsync(this.DeviceId, qos1Notification);
            });

            Task timeoutTask = Task.Run(async () =>
            {
                await Task.Delay(TestTimeout);
                throw new TimeoutException("Test has timed out.");
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
                hubConnectionStringBuilder.HostName,
                new DeviceAuthenticationWithRegistrySymmetricKey(this.DeviceId, device.Authentication.SymmetricKey.PrimaryKey));
            Client.Message message = await deviceClient.ReceiveAsync(TimeSpan.FromSeconds(1));
            Assert.IsTrue(message == null, string.Format("Message is not null: {0}", message));

        }

        static IEnumerable<TestScenarioStep> GetScenarioForCloudToDeviceQOS2(
            Func<object> currentMessageFunc, 
            string devicIdlocal, 
            string password, 
            string clientIdlocal
            )
        {
            // Connect with clean session set to false.
            yield return TestScenarioStep.Write(new ConnectPacket
            {
                ClientId = clientIdlocal,
                CleanSession = false,
                HasUsername = true,
                Username = devicIdlocal,
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
                        new SubscriptionRequest(string.Format("/{0}/{1}/TCU_COMMAND","NA",clientIdlocal), QualityOfService.ExactlyOnce),
                        new SubscriptionRequest(string.Format("/{0}/{1}/SECURITY/SERVER_ERROR","NA",clientIdlocal), QualityOfService.AtLeastOnce)
                }
            });

            var subAckPacket = Xunit.Assert.IsType<SubAckPacket>(currentMessageFunc());
            Xunit.Assert.Equal(subscribePacketId, subAckPacket.PacketId);
            Xunit.Assert.Equal(2, subAckPacket.ReturnCodes.Count);
            Xunit.Assert.Equal(QualityOfService.ExactlyOnce, subAckPacket.ReturnCodes[0]);
            Xunit.Assert.Equal(QualityOfService.AtLeastOnce, subAckPacket.ReturnCodes[1]);

            int publishQoS1PacketId = GetRandomPacketId();
            yield return TestScenarioStep.Write(
                new PublishPacket(QualityOfService.AtMostOnce, false, false)
                {
                    TopicName = string.Format("{0}/{1}/TCU_ALERT", "NA", clientIdlocal),
                    Payload = Unpooled.WrappedBuffer(Encoding.UTF8.GetBytes("{\"test\": \"telemetry-QoS0\"}"))
                }
                ,
                new PublishPacket(QualityOfService.AtLeastOnce, false, false)
                {
                    PacketId = publishQoS1PacketId,
                    TopicName = string.Format("{0}/{1}/TCU_ALERT", "NA", devicIdlocal),
                    Payload = Unpooled.WrappedBuffer(Encoding.UTF8.GetBytes("{\"test\": \"telemetry\"}"))
                });

            var packets = new Packet[3];
            for (int i = packets.Length - 1; i >= 0; i--)
            {
                packets[i] = Xunit.Assert.IsAssignableFrom<Packet>(currentMessageFunc());
                if (i > 0)
                {
                    yield return TestScenarioStep.ReadMore();
                }
            }

            PubAckPacket pubAckPacket = Xunit.Assert.Single(packets.OfType<PubAckPacket>());
            Assert.AreEqual(publishQoS1PacketId, pubAckPacket.PacketId);

            PublishPacket publishQoS1Packet = Xunit.Assert.Single(packets.OfType<PublishPacket>().Where(x => x.QualityOfService == QualityOfService.AtLeastOnce));
            Assert.AreEqual(string.Format("/{0}/{1}/SECURITY/SERVER_ERROR", "NA", clientIdlocal), publishQoS1Packet.TopicName);
            Assert.AreEqual(NotificationQoS1Content, Encoding.UTF8.GetString(publishQoS1Packet.Payload.ToArray()));

            PublishPacket publishQoS2Packet = Xunit.Assert.Single(packets.OfType<PublishPacket>().Where(x => x.QualityOfService == QualityOfService.ExactlyOnce));
            Assert.AreEqual(string.Format("/{0}/{1}/TCU_COMMAND", "NA", clientIdlocal), publishQoS2Packet.TopicName);
            Assert.AreEqual(NotificationQoS2Content, Encoding.UTF8.GetString(publishQoS2Packet.Payload.ToArray()));

            yield return TestScenarioStep.Write(PubAckPacket.InResponseTo(publishQoS1Packet),PubRecPacket.InResponseTo(publishQoS2Packet));

            var pubRelQoS2Packet = Xunit.Assert.IsAssignableFrom<PubRelPacket>(currentMessageFunc());
            Xunit.Assert.Equal(publishQoS2Packet.PacketId, pubRelQoS2Packet.PacketId);
                yield return TestScenarioStep.Write(false, PubCompPacket.InResponseTo(pubRelQoS2Packet), DisconnectPacket.Instance);
           

        }
    }
}
