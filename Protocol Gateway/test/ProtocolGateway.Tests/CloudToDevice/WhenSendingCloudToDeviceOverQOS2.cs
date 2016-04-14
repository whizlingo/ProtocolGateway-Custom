using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
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
using Microsoft.Azure.Devices.ProtocolGateway.Providers.CloudStorage;
using Microsoft.ServiceBus.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using TaskExtensions = Microsoft.Azure.Devices.Gateway.Tests.Extensions.TaskExtensions;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;


namespace Microsoft.Azure.Devices.ProtocolGateway.Tests.CloudToDevice
{
    [TestClass]
    public class WhenSendingCloudToDeviceOverQos2 : TestsBase
    {
        private CloudTable table;

        /// <summary>
        /// Should verify that the message delivery from cloud to device over qos2.
        /// </summary>
        [TestMethod]
        public async Task ShouldVerifyMessageDeliveryFromCloudToDeviceOverQos2()
        {
            IotHubConnectionStringBuilder hubConnectionStringBuilder = IotHubConnectionStringBuilder.Create(this.IotHubConnectionString);
            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(this.IotHubConnectionString);
            Device device = await registryManager.GetDeviceAsync(this.DeviceId);

            EventHubClient eventHubClient = EventHubClient.CreateFromConnectionString(this.IotHubConnectionString, "messages/events");
            EventHubConsumerGroup ehGroup = eventHubClient.GetConsumerGroup("test");

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

        static IEnumerable<TestScenarioStep> GetScenarioForCloudToDeviceQOS2(Func<object> currentMessageFunc, string devicIdlocal, string password, string clientIdlocal, bool shouldDisconnectBeforeSendingPubComp = false)
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
                        new SubscriptionRequest(string.Format("/{0}/{1}/TCU_COMMAND","NA",clientIdlocal), QualityOfService.ExactlyOnce)
                }
            });

            var subAckPacket = Xunit.Assert.IsType<SubAckPacket>(currentMessageFunc());
            Xunit.Assert.Equal(subscribePacketId, subAckPacket.PacketId);
            Xunit.Assert.Equal(1, subAckPacket.ReturnCodes.Count);
            Xunit.Assert.Equal(QualityOfService.ExactlyOnce, subAckPacket.ReturnCodes[0]);

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

            var packets = new Packet[2];
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

            PublishPacket publishQoS2Packet = Xunit.Assert.Single(packets.OfType<PublishPacket>().Where(x => x.QualityOfService == QualityOfService.ExactlyOnce));
            Assert.AreEqual(string.Format("/{0}/{1}/TCU_COMMAND", "NA", clientIdlocal), publishQoS2Packet.TopicName);
            Assert.AreEqual(NotificationQoS2Content, Encoding.UTF8.GetString(publishQoS2Packet.Payload.ToArray()));

            yield return TestScenarioStep.Write(PubRecPacket.InResponseTo(publishQoS2Packet));

            var pubRelQoS2Packet = Xunit.Assert.IsAssignableFrom<PubRelPacket>(currentMessageFunc());
            Xunit.Assert.Equal(publishQoS2Packet.PacketId, pubRelQoS2Packet.PacketId);
            if (shouldDisconnectBeforeSendingPubComp)
            {
                yield return TestScenarioStep.Write(false, DisconnectPacket.Instance);
            }
            else
            {
                yield return TestScenarioStep.Write(false, PubCompPacket.InResponseTo(pubRelQoS2Packet), DisconnectPacket.Instance);
            }

        }

        [TestMethod]
        public async Task ShouldVerifyThatQos2MessagesAreRetriedIfDeviceDisconnectsDuringThePreviousDelivery()
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
                    new TestScenarioRunner(cmf => GetScenarioForConnectAndStartQOS2PublishAndDisconnectInBetween(cmf,
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

                Tuple<EventData, string> qos0Event = ehMessages.Where(x => TelemetryQoS0Content.Equals(x.Item2, StringComparison.Ordinal)).Single();

                Tuple<EventData, string> qos1Event = ehMessages.Where(x => TelemetryQoS1Content.Equals(x.Item2, StringComparison.Ordinal)).Single();

                string qosPropertyName = ConfigurationManager.AppSettings["QoSPropertyName"];

                var qos2Notification = new Message(Encoding.UTF8.GetBytes(NotificationQoS2Content));
                qos2Notification.Properties[qosPropertyName] = "2";
                qos2Notification.Properties["subTopic"] = "critical-alert";
                qos2Notification.Properties["region"] = "NA";
                qos2Notification.MessageId = Guid.NewGuid().ToString();
                qos2Notification.Properties["vin"] = this.ClientId;
                qos2Notification.Properties["TopicName"] = "TCU_COMMAND";
                await serviceClient.SendAsync(this.DeviceId, qos2Notification);
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

            // Connecting the client again.
            var groupforSecondTimeConnection = new MultithreadEventLoopGroup();
            string targetHostForSecondTimeConnection = this.TlsCertificate.GetNameInfo(X509NameType.DnsName, false);
            Bootstrap bootstrapForSecondTimeConnection = new Bootstrap()
                .Group(groupforSecondTimeConnection)
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Handler(new ActionChannelInitializer<ISocketChannel>(ch => ch.Pipeline.AddLast(
                    TlsHandler.Client(targetHostForSecondTimeConnection, null, (sender, certificate, chain, errors) => true),
                    MqttEncoder.Instance,
                    new MqttDecoder(false, 256 * 1024),
                    new TestScenarioRunner(
                        cmf => GetClientScenarioForResumingTheQOS2PublishAgainAfterConnectAndComplete(cmf, this.DeviceId, device.Authentication.SymmetricKey.PrimaryKey, this.ClientId),
                        testPromise,
                        CommunicationTimeout,
                        CommunicationTimeout),
                    new XUnitLoggingHandler(this.output))));
            IChannel clientChannelForSecondTimeConnection = await bootstrapForSecondTimeConnection.ConnectAsync(this.ServerAddress, this.ProtocolGatewayPort);
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
                hubConnectionStringBuilder.HostName,
                new DeviceAuthenticationWithRegistrySymmetricKey(this.DeviceId, device.Authentication.SymmetricKey.PrimaryKey));
            Client.Message message = await deviceClient.ReceiveAsync(TimeSpan.FromSeconds(1));
            Assert.IsTrue(message == null, string.Format("Message is not null: {0}", message));
        }

        /// <summary>
        /// Gets the client scenario for resuming the qo s2 publish again after connect and complete.
        /// </summary>
        /// <param name="currentMessageFunc">The current message function.</param>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="password">The password.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <returns></returns>
        static IEnumerable<TestScenarioStep> GetClientScenarioForResumingTheQOS2PublishAgainAfterConnectAndComplete(
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
                        new SubscriptionRequest(string.Format("/{0}/{1}/TCU_COMMAND","NA",clientId), QualityOfService.ExactlyOnce)
                }
            });

            var packets = new Packet[2];
            for (int i = packets.Length - 1; i >= 0; i--)
            {
                packets[i] = Xunit.Assert.IsAssignableFrom<Packet>(currentMessageFunc());
                if (i > 0)
                {
                    yield return TestScenarioStep.ReadMore();
                }
            }

            // Check that the Server sends does the pub rel packet for the QOS 2 message again.
            PubRelPacket pubRelPacket = Xunit.Assert.Single(packets.OfType<PubRelPacket>().Where(x => x.QualityOfService == QualityOfService.AtLeastOnce));

            yield return TestScenarioStep.Write(
            false, // it is a final step and we do not expect response
            PubCompPacket.InResponseTo(pubRelPacket),
            DisconnectPacket.Instance);
        }

        /// <summary>
        /// Gets the scenario for connect and start qo s2 publish and disconnect in between.
        /// </summary>
        /// <param name="currentMessageFunc">The current message function.</param>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="password">The password.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <returns></returns>
        static IEnumerable<TestScenarioStep> GetScenarioForConnectAndStartQOS2PublishAndDisconnectInBetween(
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

            var connAckPacket = Xunit.Assert.IsType<ConnAckPacket>(currentMessageFunc());
            Xunit.Assert.Equal(ConnectReturnCode.Accepted, connAckPacket.ReturnCode);

            int subscribePacketId = GetRandomPacketId();
            yield return TestScenarioStep.Write(new SubscribePacket
            {
                PacketId = subscribePacketId,
                Requests = new[]
                {
                        new SubscriptionRequest(string.Format("/{0}/{1}/TCU_COMMAND","NA",clientId), QualityOfService.ExactlyOnce)
                }
            });

            var subAckPacket = Xunit.Assert.IsType<SubAckPacket>(currentMessageFunc());
            Xunit.Assert.Equal(subscribePacketId, subAckPacket.PacketId);
            Xunit.Assert.Equal(1, subAckPacket.ReturnCodes.Count);
            Xunit.Assert.Equal(QualityOfService.ExactlyOnce, subAckPacket.ReturnCodes[0]);

            int publishQoS1PacketId = GetRandomPacketId();
            yield return TestScenarioStep.Write(
                new PublishPacket(QualityOfService.AtMostOnce, false, false)
                {
                    TopicName = string.Format("{0}/{1}/TCU_ALERT", "NA", clientId),
                    Payload = Unpooled.WrappedBuffer(Encoding.UTF8.GetBytes("{\"test\": \"telemetry-QoS0\"}"))
                }
                ,
                new PublishPacket(QualityOfService.AtLeastOnce, false, false)
                {
                    PacketId = publishQoS1PacketId,
                    TopicName = string.Format("{0}/{1}/TCU_ALERT", "NA", deviceId),
                    Payload = Unpooled.WrappedBuffer(Encoding.UTF8.GetBytes("{\"test\": \"telemetry\"}"))
                });

            var packets = new Packet[2];
            for (int i = packets.Length - 1; i >= 0; i--)
            {
                packets[i] = Xunit.Assert.IsAssignableFrom<Packet>(currentMessageFunc());
                if (i > 0)
                {
                    yield return TestScenarioStep.ReadMore();
                }
            }

            PubAckPacket pubAckPacket = Xunit.Assert.Single(packets.OfType<PubAckPacket>());
            Xunit.Assert.Equal(publishQoS1PacketId, pubAckPacket.PacketId);

            PublishPacket publishQoS2Packet = Xunit.Assert.Single(packets.OfType<PublishPacket>().Where(x => x.QualityOfService == QualityOfService.ExactlyOnce));
            Xunit.Assert.Equal(string.Format("/{0}/{1}/TCU_COMMAND", "NA", clientId), publishQoS2Packet.TopicName);
            Xunit.Assert.Equal(NotificationQoS2Content, Encoding.UTF8.GetString(publishQoS2Packet.Payload.ToArray()));

            yield return TestScenarioStep.Write(PubRecPacket.InResponseTo(publishQoS2Packet));



            var pubRelQoS2Packet = Xunit.Assert.IsAssignableFrom<PubRelPacket>(currentMessageFunc());
            Xunit.Assert.Equal(publishQoS2Packet.PacketId, pubRelQoS2Packet.PacketId);

            //string storageConectionString = "storageConnectionString";
            //string tableName ="tablename";
            //CloudStorageAccount cloudStorageAccount;
            //CloudStorageAccount.TryParse(storageConectionString, out cloudStorageAccount);
            //CloudTableClient tableClient = cloudStorageAccount.CreateCloudTableClient();
            //this.table = tableClient.GetTableReference(tableName);

            yield return TestScenarioStep.Write(false, DisconnectPacket.Instance);
        }


        

        static IEnumerable<TestScenarioStep> GetScenarioForServerFailsToStorePubRecAndRePublishMessageConnect(
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

            var connAckPacket = Xunit.Assert.IsType<ConnAckPacket>(currentMessageFunc());
            Xunit.Assert.Equal(ConnectReturnCode.Accepted, connAckPacket.ReturnCode);

            int subscribePacketId = GetRandomPacketId();
            yield return TestScenarioStep.Write(new SubscribePacket
            {
                PacketId = subscribePacketId,
                Requests = new[]
                {
                        new SubscriptionRequest(string.Format("/{0}/{1}/TCU_COMMAND","NA",clientId), QualityOfService.ExactlyOnce)
                }
            });

            var subAckPacket = Xunit.Assert.IsType<SubAckPacket>(currentMessageFunc());
            Xunit.Assert.Equal(subscribePacketId, subAckPacket.PacketId);
            Xunit.Assert.Equal(1, subAckPacket.ReturnCodes.Count);
            Xunit.Assert.Equal(QualityOfService.ExactlyOnce, subAckPacket.ReturnCodes[0]);

            int publishQoS1PacketId = GetRandomPacketId();
            yield return TestScenarioStep.Write(
                new PublishPacket(QualityOfService.AtMostOnce, false, false)
                {
                    TopicName = string.Format("{0}/{1}/TCU_ALERT", "NA", clientId),
                    Payload = Unpooled.WrappedBuffer(Encoding.UTF8.GetBytes("{\"test\": \"telemetry-QoS0\"}"))
                }
                ,
                new PublishPacket(QualityOfService.AtLeastOnce, false, false)
                {
                    PacketId = publishQoS1PacketId,
                    TopicName = string.Format("{0}/{1}/TCU_ALERT", "NA", deviceId),
                    Payload = Unpooled.WrappedBuffer(Encoding.UTF8.GetBytes("{\"test\": \"telemetry\"}"))
                });

            var packets = new Packet[2];
            for (int i = packets.Length - 1; i >= 0; i--)
            {
                packets[i] = Xunit.Assert.IsAssignableFrom<Packet>(currentMessageFunc());
                if (i > 0)
                {
                    yield return TestScenarioStep.ReadMore();
                }
            }

            PubAckPacket pubAckPacket = Xunit.Assert.Single(packets.OfType<PubAckPacket>());
            Xunit.Assert.Equal(publishQoS1PacketId, pubAckPacket.PacketId);

            PublishPacket publishQoS2Packet = Xunit.Assert.Single(packets.OfType<PublishPacket>().Where(x => x.QualityOfService == QualityOfService.ExactlyOnce));
            Xunit.Assert.Equal(string.Format("/{0}/{1}/TCU_COMMAND", "NA", clientId), publishQoS2Packet.TopicName);
            Xunit.Assert.Equal(NotificationQoS2Content, Encoding.UTF8.GetString(publishQoS2Packet.Payload.ToArray()));

            try
            {
                 TestScenarioStep.Write(PubRecPacket.InResponseTo(publishQoS2Packet));
            }
            catch
            { }

            //var pubRelQoS2Packet = Xunit.Assert.IsAssignableFrom<PubRelPacket>(currentMessageFunc());
            //Xunit.Assert.Equal(publishQoS2Packet.PacketId, pubRelQoS2Packet.PacketId);
            
           // yield return TestScenarioStep.Write(false, DisconnectPacket.Instance);
        }

        [TestMethod]
        public async Task ShouldVerifyMessageDeliveryForPubrecFailFromCloudToDeviceOverQos2()
        {
            IotHubConnectionStringBuilder hubConnectionStringBuilder = IotHubConnectionStringBuilder.Create(this.IotHubConnectionString);
            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(this.IotHubConnectionString);
            Device device = await registryManager.GetDeviceAsync(this.DeviceId);

            EventHubClient eventHubClient = EventHubClient.CreateFromConnectionString(this.IotHubConnectionString, "messages/events");
            EventHubConsumerGroup ehGroup = eventHubClient.GetConsumerGroup("test");

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
                    new TestScenarioRunner(cmf => GetScenarioForServerFailsToStorePubRecAndRePublishMessageConnect(cmf,
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

            // Connecting the client again.
            var groupforSecondTimeConnection = new MultithreadEventLoopGroup();
            string targetHostForSecondTimeConnection = this.TlsCertificate.GetNameInfo(X509NameType.DnsName, false);
            Bootstrap bootstrapForSecondTimeConnection = new Bootstrap()
                       .Option(ChannelOption.TcpNodelay, true)
                         .Group(groupforSecondTimeConnection)
                .Channel<TcpSocketChannel>()
.Handler(new ActionChannelInitializer<ISocketChannel>(ch => ch.Pipeline.AddLast(
                    TlsHandler.Client(targetHostForSecondTimeConnection, null, (sender, certificate, chain, errors) => true),
                    MqttEncoder.Instance,
                    new MqttDecoder(false, 256 * 1024),
                    new TestScenarioRunner(
                        cmf => GetClientScenarioForPublishAgainQ0s2AfterConnectAndComplete(cmf, this.DeviceId, device.Authentication.SymmetricKey.PrimaryKey, this.ClientId),
                        testPromise,
                        CommunicationTimeout,
                        CommunicationTimeout),
                    new XUnitLoggingHandler(this.output))));
            IChannel clientChannelForSecondTimeConnection = await bootstrapForSecondTimeConnection.ConnectAsync(this.ServerAddress, this.ProtocolGatewayPort);
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
        }

        static IEnumerable<TestScenarioStep> GetClientScenarioForPublishAgainQ0s2AfterConnectAndComplete(
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
                        new SubscriptionRequest(string.Format("/{0}/{1}/TCU_COMMAND","NA",clientId), QualityOfService.ExactlyOnce)
                }
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

            // Check that the Server sends does the pub rel packet for the QOS 2 message again.
            PublishPacket pubRelPacket = Xunit.Assert.Single(packets.OfType<PublishPacket>().Where(x => x.QualityOfService == QualityOfService.AtLeastOnce));

            yield return TestScenarioStep.Write(
            false, // it is a final step and we do not expect response
            DisconnectPacket.Instance);
        }



        //  PubComp
        static IEnumerable<TestScenarioStep> GetScenarioForServerFailsToStorePubCompAndRePublishPubRel(
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

            var connAckPacket = Xunit.Assert.IsType<ConnAckPacket>(currentMessageFunc());
            Xunit.Assert.Equal(ConnectReturnCode.Accepted, connAckPacket.ReturnCode);

            int subscribePacketId = GetRandomPacketId();
            yield return TestScenarioStep.Write(new SubscribePacket
            {
                PacketId = subscribePacketId,
                Requests = new[]
                {
                        new SubscriptionRequest(string.Format("/{0}/{1}/TCU_COMMAND","NA",clientId), QualityOfService.ExactlyOnce)
                }
            });

            var subAckPacket = Xunit.Assert.IsType<SubAckPacket>(currentMessageFunc());
            Xunit.Assert.Equal(subscribePacketId, subAckPacket.PacketId);
            Xunit.Assert.Equal(1, subAckPacket.ReturnCodes.Count);
            Xunit.Assert.Equal(QualityOfService.ExactlyOnce, subAckPacket.ReturnCodes[0]);

            int publishQoS1PacketId = GetRandomPacketId();
            yield return TestScenarioStep.Write(
                new PublishPacket(QualityOfService.AtMostOnce, false, false)
                {
                    TopicName = string.Format("{0}/{1}/TCU_ALERT", "NA", clientId),
                    Payload = Unpooled.WrappedBuffer(Encoding.UTF8.GetBytes("{\"test\": \"telemetry-QoS0\"}"))
                }
                ,
                new PublishPacket(QualityOfService.AtLeastOnce, false, false)
                {
                    PacketId = publishQoS1PacketId,
                    TopicName = string.Format("{0}/{1}/TCU_ALERT", "NA", deviceId),
                    Payload = Unpooled.WrappedBuffer(Encoding.UTF8.GetBytes("{\"test\": \"telemetry\"}"))
                });

            var packets = new Packet[2];
            for (int i = packets.Length - 1; i >= 0; i--)
            {
                packets[i] = Xunit.Assert.IsAssignableFrom<Packet>(currentMessageFunc());
                if (i > 0)
                {
                    yield return TestScenarioStep.ReadMore();
                }
            }

            PubAckPacket pubAckPacket = Xunit.Assert.Single(packets.OfType<PubAckPacket>());
            Xunit.Assert.Equal(publishQoS1PacketId, pubAckPacket.PacketId);

            PublishPacket publishQoS2Packet = Xunit.Assert.Single(packets.OfType<PublishPacket>().Where(x => x.QualityOfService == QualityOfService.ExactlyOnce));
            Xunit.Assert.Equal(string.Format("/{0}/{1}/TCU_COMMAND", "NA", clientId), publishQoS2Packet.TopicName);
            Xunit.Assert.Equal(NotificationQoS2Content, Encoding.UTF8.GetString(publishQoS2Packet.Payload.ToArray()));

           yield return TestScenarioStep.Write(PubRecPacket.InResponseTo(publishQoS2Packet));
            
            var pubRelQoS2Packet = Xunit.Assert.IsAssignableFrom<PubRelPacket>(currentMessageFunc());
            Xunit.Assert.Equal(publishQoS2Packet.PacketId, pubRelQoS2Packet.PacketId);
            try
            {
                TestScenarioStep.Write(false, PubCompPacket.InResponseTo(pubRelQoS2Packet));
            }
            catch
            { }


            // yield return TestScenarioStep.Write(false, DisconnectPacket.Instance);
        }

        [TestMethod]
        public async Task ShouldVerifyMessageDeliveryForPubCompDeleteFromCloudToDeviceOverQos2()
        {
            IotHubConnectionStringBuilder hubConnectionStringBuilder = IotHubConnectionStringBuilder.Create(this.IotHubConnectionString);
            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(this.IotHubConnectionString);
            Device device = await registryManager.GetDeviceAsync(this.DeviceId);

            EventHubClient eventHubClient = EventHubClient.CreateFromConnectionString(this.IotHubConnectionString, "messages/events");
            EventHubConsumerGroup ehGroup = eventHubClient.GetConsumerGroup("test");

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
                    new TestScenarioRunner(cmf => GetScenarioForServerFailsToStorePubCompAndRePublishPubRel(cmf,
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

            // Connecting the client again.
            var groupforSecondTimeConnection = new MultithreadEventLoopGroup();
            string targetHostForSecondTimeConnection = this.TlsCertificate.GetNameInfo(X509NameType.DnsName, false);
            Bootstrap bootstrapForSecondTimeConnection = new Bootstrap()
                       .Option(ChannelOption.TcpNodelay, true)
                         .Group(groupforSecondTimeConnection)
                .Channel<TcpSocketChannel>()
.Handler(new ActionChannelInitializer<ISocketChannel>(ch => ch.Pipeline.AddLast(
                    TlsHandler.Client(targetHostForSecondTimeConnection, null, (sender, certificate, chain, errors) => true),
                    MqttEncoder.Instance,
                    new MqttDecoder(false, 256 * 1024),
                    new TestScenarioRunner(
                        cmf => GetClientScenarioForPubRelAgainQ0s2AfterConnectAndComplete(cmf, this.DeviceId, device.Authentication.SymmetricKey.PrimaryKey, this.ClientId),
                        testPromise,
                        CommunicationTimeout,
                        CommunicationTimeout),
                    new XUnitLoggingHandler(this.output))));
            IChannel clientChannelForSecondTimeConnection = await bootstrapForSecondTimeConnection.ConnectAsync(this.ServerAddress, this.ProtocolGatewayPort);
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
        }

        static IEnumerable<TestScenarioStep> GetClientScenarioForPubRelAgainQ0s2AfterConnectAndComplete(
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
                        new SubscriptionRequest(string.Format("/{0}/{1}/TCU_COMMAND","NA",clientId), QualityOfService.ExactlyOnce)
                }
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

            // Check that the Server sends does the pub Comp packet for the QOS 2 message again.
            PubRelPacket pubRelPacket = Xunit.Assert.Single(packets.OfType<PubRelPacket>().Where(x => x.QualityOfService == QualityOfService.AtLeastOnce));

            yield return TestScenarioStep.Write(
            false, // it is a final step and we do not expect response
            DisconnectPacket.Instance);
        }



    }
}
