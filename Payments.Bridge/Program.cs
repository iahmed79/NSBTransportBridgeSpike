namespace Payments.Bridge
{
    using System;
    using System.Data.SqlClient;
    using System.Threading.Tasks;

    using NServiceBus;
    using NServiceBus.Bridge;
    using NServiceBus.Configuration.AdvancedExtensibility;
    using NServiceBus.Serialization;
    using NServiceBus.Settings;

    class Program
    {
        static async Task Main()
        {
            var dbConnectionString = Environment.GetEnvironmentVariable("NSBCompat_DBConnectionString");
            var asbConnectionString = Environment.GetEnvironmentVariable("NSBCompat_ASBConnectionString");

            var bridgeConfiguration = NServiceBus.Bridge.Bridge
                .Between<MsmqTransport>(
                    endpointName: "payments.bridge.endpoint.msmq",
                    customization: transportExtensions =>
                    {
                        transportExtensions.Transactions(TransportTransactionMode.ReceiveOnly);
                    })
                .And<AzureServiceBusTransport>(
                    endpointName: "payments.bridge.endpoint.asb",
                    customization: transportExtensions =>
                    {
                        transportExtensions.ConnectionString(asbConnectionString);
                        transportExtensions.Transactions(TransportTransactionMode.ReceiveOnly);
                        transportExtensions.UseForwardingTopology();
                        var settings = transportExtensions.GetSettings();
                        var serializer = Tuple.Create(new NewtonsoftSerializer() as SerializationDefinition, new SettingsHolder());
                        settings.Set("MainSerializer", serializer);
                    });

            bridgeConfiguration.AutoCreateQueues();
            var storage = new SqlSubscriptionStorage(() => new SqlConnection(dbConnectionString), "Bridge", new SqlDialect.MsSqlServer(), null);
            await storage.Install();
            bridgeConfiguration.UseSubscriptionPersistence(storage);

            var bridge = bridgeConfiguration.Create();
            
            await bridge.Start();

            Console.ReadLine();
        }
    }
}
