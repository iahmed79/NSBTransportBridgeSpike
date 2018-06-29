namespace Payments.Bridge
{
    using System;
    using System.Data.SqlClient;
    using System.Threading.Tasks;

    using NServiceBus;
    using NServiceBus.Configuration.AdvancedExtensibility;
    using NServiceBus.Router;
    using NServiceBus.Serialization;
    using NServiceBus.Settings;

    class Program
    {
        static async Task Main()
        {
            var dbConnectionString = Environment.GetEnvironmentVariable("NSBCompat_DBConnectionString");
            var asbConnectionString = Environment.GetEnvironmentVariable("NSBCompat_ASBConnectionString");

            var storage = new SqlSubscriptionStorage(() => new SqlConnection(dbConnectionString), "Bridge", new SqlDialect.MsSqlServer(), null);
            await storage.Install();

            var routerConfig = new RouterConfiguration("Payments.Bridge");
            routerConfig.AddInterface<MsmqTransport>(
                "Left",
                extensions =>
                    {
                        extensions.Transactions(TransportTransactionMode.ReceiveOnly);
                    })
                .UseSubscriptionPersistence(storage);

            routerConfig.AddInterface<AzureServiceBusTransport>(
                "Right",
                extensions =>
                    {
                        extensions.ConnectionString(asbConnectionString);
                        extensions.Transactions(TransportTransactionMode.ReceiveOnly);
                        extensions.UseForwardingTopology();
                        extensions.Sanitization().UseStrategy<SubscriptionRuleNameSanitizationStrategy>();
                        var settings = extensions.GetSettings();
                        var serializer = Tuple.Create(new NewtonsoftSerializer() as SerializationDefinition, new SettingsHolder());
                        settings.Set("MainSerializer", serializer);
                    })
                .UseSubscriptionPersistence(storage);
            
            routerConfig.AutoCreateQueues();
            
            var staticRouting = routerConfig.UseStaticRoutingProtocol();
            staticRouting.AddForwardRoute("Left", "Right");
            staticRouting.AddForwardRoute("Right", "Left");

            var router = Router.Create(routerConfig);
            
            await router.Start();
            
            Console.ReadLine();
        }
    }
}
