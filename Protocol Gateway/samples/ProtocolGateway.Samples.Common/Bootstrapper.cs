// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DotNetty.Handlers.Tls;

namespace ProtocolGateway.Samples.Common
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Buffers;
    using DotNetty.Codecs.Mqtt;
    using DotNetty.Common.Concurrency;
    using DotNetty.Transport.Bootstrapping;
    using DotNetty.Transport.Channels;
    using DotNetty.Transport.Channels.Sockets;
    using Microsoft.Azure.Devices.ProtocolGateway;
    using Microsoft.Azure.Devices.ProtocolGateway.Instrumentation;
    using Microsoft.Azure.Devices.ProtocolGateway.Mqtt;
    using Microsoft.Azure.Devices.ProtocolGateway.Mqtt.Auth;
    using Microsoft.Azure.Devices.ProtocolGateway.Mqtt.Persistence;
    using Microsoft.Azure.Devices.ProtocolGateway.Mqtt.Routing;


    public class Bootstrapper
    {
        const int MqttsPort = 8883;
        const int MqttPort = 1883;
        const int ListenBacklogSize = 200; // connections allowed pending accept
        const int IotHubConnectionsPerThread = 200; // IoT Hub connections per thread

        readonly TaskCompletionSource closeCompletionSource;
        readonly ISettingsProvider settingsProvider;
        readonly Settings settings;
        readonly ISessionStatePersistenceProvider sessionStateManager;
        readonly IQos2StatePersistenceProvider qos2StateProvider;
        readonly IAuthenticationProvider authProvider;
        readonly ITopicNameRouter topicNameRouter;
        X509Certificate2 tlsCertificate;
        IEventLoopGroup parentEventLoopGroup;
        IEventLoopGroup eventLoopGroup;
        IByteBufferAllocator bufferAllocator;
        ICollection<IChannel> serverChannels;

        public Bootstrapper(ISettingsProvider settingsProvider, ISessionStatePersistenceProvider sessionStateManager, IQos2StatePersistenceProvider qos2StateProvider)
        {
            Contract.Requires(settingsProvider != null);
            Contract.Requires(sessionStateManager != null);

            this.closeCompletionSource = new TaskCompletionSource();

            this.settingsProvider = settingsProvider;
            this.settings = new Settings(this.settingsProvider);
            this.sessionStateManager = sessionStateManager;
            this.qos2StateProvider = qos2StateProvider;
            this.authProvider = new DeviceKeyAuthenticationProvider();
            this.topicNameRouter = new TopicNameRouter();
            this.serverChannels = new List<IChannel>();
        }

        public Task CloseCompletion
        {
            get { return this.closeCompletionSource.Task; }
        }

        public async Task RunAsync(X509Certificate2 certificate, int threadCount, CancellationToken cancellationToken)
        {
            Contract.Requires(certificate != null);
            Contract.Requires(threadCount > 0);

            try
            {
                BootstrapperEventSource.Log.Info("Starting", null);

                PerformanceCounters.ConnectionsEstablishedTotal.RawValue = 0;
                PerformanceCounters.ConnectionsCurrent.RawValue = 0;

                this.tlsCertificate = certificate;
                this.parentEventLoopGroup = new MultithreadEventLoopGroup(1);
                this.eventLoopGroup = new MultithreadEventLoopGroup(threadCount);
                this.bufferAllocator = new PooledByteBufferAllocator(16 * 1024, 300 * 1024 * 1024 / threadCount); // reserve up to 300 MB of 16 KB buffers

                ServerBootstrap bootstrap = this.SetupBootstrap();
                var isNonTlsEnabled = settingsProvider.GetBooleanSetting("IsNonTLSEnabled", false);
                BootstrapperEventSource.Log.Info(string.Format(" Initialize TLS endpoint on port {0} with certificate {1}.", MqttsPort, this.tlsCertificate.Thumbprint), null);
                var channel = await bootstrap.BindAsync(IPAddress.Any, MqttsPort);
                this.serverChannels.Add(channel);

                if (isNonTlsEnabled)
                {
                    BootstrapperEventSource.Log.Info(string.Format("Initializing endpoint on port {0}.", MqttPort), null);
                    var nonTlsChannel = await bootstrap.BindAsync(IPAddress.Any, MqttPort);
                    this.serverChannels.Add(nonTlsChannel);
                }

                cancellationToken.Register(this.CloseAsync);

                BootstrapperEventSource.Log.Info("Started", null);
            }
            catch (Exception ex)
            {
                BootstrapperEventSource.Log.Error("Failed to start", ex);
                this.CloseAsync();
            }
        }

        async void CloseAsync()
        {
            try
            {
                BootstrapperEventSource.Log.Info("Stopping", null);
                if (this.serverChannels != null)
                {
                    foreach (var channel in serverChannels)
                    {
                        await channel.CloseAsync();
                    }
                }
                if (this.eventLoopGroup != null)
                {
                    await this.eventLoopGroup.ShutdownGracefullyAsync();
                }

                BootstrapperEventSource.Log.Info("Stopped", null);
            }
            catch (Exception ex)
            {
                BootstrapperEventSource.Log.Warning("Failed to stop cleanly", ex);
            }
            finally
            {
                this.closeCompletionSource.TryComplete();
            }
        }

        ServerBootstrap SetupBootstrap()
        {
            int maxInboundMessageSize = this.settingsProvider.GetIntegerSetting("MaxInboundMessageSize", 256 * 1024);
            string connectionString = this.settings.IotHubConnectionString;
            if (connectionString.IndexOf("DeviceId=", StringComparison.OrdinalIgnoreCase) == -1)
            {
                connectionString += ";DeviceId=stub";
            }

            var deviceClientFactory = new ThreadLocal<DeviceClientFactoryFunc>(() =>
            {
                string poolId = Guid.NewGuid().ToString("N");
                return IotHubDeviceClient.PreparePoolFactory(connectionString, poolId, IotHubConnectionsPerThread);
            });
            return new ServerBootstrap()
                .Group(this.parentEventLoopGroup, this.eventLoopGroup)
                .Option(ChannelOption.SoBacklog, ListenBacklogSize)
                .ChildOption(ChannelOption.Allocator, this.bufferAllocator)
                .ChildOption(ChannelOption.AutoRead, false)
                .Channel<TcpServerSocketChannel>()
                .ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    if (((IPEndPoint)channel.LocalAddress).Port == MqttsPort)
                    {
                        channel.Pipeline.AddLast(TlsHandler.Server(this.tlsCertificate));
                    }
                    foreach (var VARIABLE in channel.Pipeline.AddLast(
                        MqttEncoder.Instance,
                        new MqttDecoder(true, maxInboundMessageSize),
                        new MqttIotHubAdapter(
                            this.settings,
                            deviceClientFactory.Value,
                            this.sessionStateManager,
                            this.authProvider,
                            this.topicNameRouter,
                            this.qos2StateProvider)))
                    {
                        
                    }
                }));
        }
    }
}