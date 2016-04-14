using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit.Abstractions;

namespace Microsoft.Azure.Devices.ProtocolGateway.Tests
{
    public class TestsBase
    {
        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        protected AppConfigSettingsProvider SettingsProvider { get; set; }
        protected int ProtocolGatewayPort { get; set; }
        protected X509Certificate2 TlsCertificate { get; set; }
        protected IPAddress ServerAddress { get; set; }

        protected Func<Task> cleanupFunc;

        protected ITestOutputHelper output;

        protected string DeviceId;

        protected string ClientId;

        public static readonly TimeSpan CommunicationTimeout = TimeSpan.FromMilliseconds(-1); //TimeSpan.FromSeconds(15);

        public static readonly TimeSpan TestTimeout = TimeSpan.FromMinutes(5); //TimeSpan.FromMinutes(1);

        protected string IotHubConnectionString;

        protected const string NotificationQoS2Content = "{\"test\": \"exactly once\"}";

        protected const string NotificationQoS1Content = "{\"test\": \"atleast once\"}";

        protected const string TelemetryQoS0Content = "{\"test\": \"telemetry-QoS0\"}";

        protected const string TelemetryQoS1Content = "{\"test\": \"telemetry\"}";

        protected void ScheduleCleanup(Func<Task> cleanupFunc)
        {
            Func<Task> currentCleanupFunc = this.cleanupFunc;
            this.cleanupFunc = async () =>
            {
                if (currentCleanupFunc != null)
                {
                    await currentCleanupFunc();
                }

                await cleanupFunc();
            };
        }

        // Dispose off any resources...
        public void Dispose()
        {
            if (this.cleanupFunc != null)
            {
                if (!this.cleanupFunc().Wait(TimeSpan.FromSeconds(10)))
                {
                    if (this.output != null)
                    {
                        this.output.WriteLine("Server cleanup did not complete in time.");
                    }
                }
            }
        }

        protected static async Task<Tuple<EventData, string>[]> CollectEventHubMessagesAsync(EventHubReceiver[] receivers, int messagesPending)
        {
            List<Task<EventData>> receiveTasks = receivers.Select(r => r.ReceiveAsync(TimeSpan.FromMinutes(20))).ToList();
            var ehMessages = new Tuple<EventData, string>[messagesPending];
            while (true)
            {
                Task<EventData> receivedTask = await Task.WhenAny(receiveTasks);
                EventData eventData = receivedTask.Result;
                if (eventData != null)
                {
                    ehMessages[messagesPending - 1] = Tuple.Create(eventData, Encoding.UTF8.GetString(eventData.GetBytes()));

                    if (--messagesPending == 0)
                    {
                        break;
                    }
                }

                int receivedIndex = receiveTasks.IndexOf(receivedTask);
                receiveTasks[receivedIndex] = receivers[receivedIndex].ReceiveAsync(TimeSpan.FromMinutes(20));
            }
            return ehMessages;
        }

        [TestInitialize]
        public void SetUp()
        {
            this.SettingsProvider = new AppConfigSettingsProvider();

            this.ProtocolGatewayPort = 8883; // todo: dynamic port choice to parallelize test runs (first free port)

            this.TlsCertificate = new X509Certificate2("mqtt.azurereykjavik.info.pfx", "azuremqtt");
            this.output = new OutputHelper(TestContext);
            string serverAddress;
            IPAddress serverIp;
            if (this.SettingsProvider.TryGetSetting("End2End.ServerAddress", out serverAddress) && IPAddress.TryParse(serverAddress, out serverIp))
            {
                this.ServerAddress = serverIp;
            }

            this.IotHubConnectionString = ConfigurationManager.AppSettings["IoTHubConnectionString"];
            this.DeviceId = ConfigurationManager.AppSettings["End2End.DeviceName"];
            this.ClientId = ConfigurationManager.AppSettings["End2End.ClientId"];
        }

        protected static int GetRandomPacketId()
        {
            return Guid.NewGuid().GetHashCode() & ushort.MaxValue;
        }
    }
}