// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ProtocolGateway.Samples.Cloud.Host
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.Tracing;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using DotNetty.Codecs.Mqtt;
    using DotNetty.Common.Concurrency;
    using DotNetty.Transport.Bootstrapping;
    using DotNetty.Transport.Channels;
    using Microsoft.Azure.Devices.ProtocolGateway;
    using Microsoft.Azure.Devices.ProtocolGateway.Instrumentation;
    using Microsoft.Azure.Devices.ProtocolGateway.Providers.CloudStorage;
    using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using ProtocolGateway.Samples.Common;

    public class WorkerRole : RoleEntryPoint
    {
        readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        public override void Run()
        {
            Trace.TraceInformation("Contoso.ProtocolGateway.Cloud.Host is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("Contoso.ProtocolGateway.Cloud.Host has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("Contoso.ProtocolGateway.Cloud.Host is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("Contoso.ProtocolGateway.Cloud.Host has stopped");
        }

        async Task RunAsync(CancellationToken cancellationToken)
        {
            var settingsProvider = new RoleEnvironmentSettingsProvider();

            var eventListener = new ObservableEventListener();
            eventListener.LogToWindowsAzureTable(RoleEnvironment.CurrentRoleInstance.Id, settingsProvider.GetSetting("BlobSessionStatePersistenceProvider.StorageConnectionString"), bufferingInterval: TimeSpan.FromMinutes(2));
            eventListener.EnableEvents(BootstrapperEventSource.Log, EventLevel.Informational);
            eventListener.EnableEvents(MqttIotHubAdapterEventSource.Log, EventLevel.Informational);
            eventListener.EnableEvents(ChannelEventSource.Log, EventLevel.Informational);
            eventListener.EnableEvents(BootstrapEventSource.Log, EventLevel.Informational);
            eventListener.EnableEvents(ExecutorEventSource.Log, EventLevel.Informational);
            eventListener.EnableEvents(MqttEventSource.Log, EventLevel.Informational);

            int minWorkerThreads;
            int minCompletionPortThreads;
            ThreadPool.GetMinThreads(out minWorkerThreads, out minCompletionPortThreads);
            ThreadPool.SetMinThreads(minWorkerThreads, Math.Max(16, minCompletionPortThreads));

            int threadCount = Environment.ProcessorCount;
            
            BlobSessionStatePersistenceProvider blobSessionStateProvider =
                await BlobSessionStatePersistenceProvider.CreateAsync(
                    settingsProvider.GetSetting("BlobSessionStatePersistenceProvider.StorageConnectionString"),
                    settingsProvider.GetSetting("BlobSessionStatePersistenceProvider.StorageContainerName"));

            TableQos2StatePersistenceProvider tableQos2StateProvider =
                await TableQos2StatePersistenceProvider.CreateAsync(
                    settingsProvider.GetSetting("TableQos2StatePersistenceProvider.StorageConnectionString"),
                    settingsProvider.GetSetting("TableQos2StatePersistenceProvider.StorageTableName"));

            var bootstrapper = new Bootstrapper(
                settingsProvider,
                blobSessionStateProvider,
                tableQos2StateProvider);

            X509Certificate2 tlsCertificate = GetTlsCertificate(settingsProvider.GetSetting("TlsCertificateThumbprint"),
                StoreName.My, StoreLocation.LocalMachine);
            
            await bootstrapper.RunAsync(tlsCertificate, threadCount, cancellationToken);
            await bootstrapper.CloseCompletion;
        }

        public static X509Certificate2 GetTlsCertificate(string thumbprint, StoreName storeName, StoreLocation storeLocation)
        {
            X509Store store = null;
            try
            {
                store = new X509Store(storeName, storeLocation);
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection certCollection = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);

                return certCollection.Count == 0 ? null : certCollection[0];
            }
            finally
            {
                if (store != null)
                {
                    store.Close();
                }
            }
        }
    }
}