using System;
using NServiceBus.Persistence;

namespace Payments.Endpoint.Worker
{
    using NServiceBus;
    using NServiceBus.Routing.Legacy;

    public class EndpointConfig : IConfigureThisEndpoint
    {
        public void Customize(EndpointConfiguration endpointConfiguration)
        {
            var dbConnectionString = Environment.GetEnvironmentVariable("NSBCompat_DBConnectionString");

            var transport = endpointConfiguration.UseTransport<MsmqTransport>();

            var persistence = endpointConfiguration.UsePersistence<NHibernatePersistence>()
                .ConnectionString(dbConnectionString);

            var conventions = endpointConfiguration.Conventions();

            conventions.DefiningCommandsAs(t => t.Namespace != null && t.Namespace.EndsWith("Commands"));
            conventions.DefiningEventsAs(t => t.Namespace != null && t.Namespace.EndsWith("Events"));

            var routing = transport.Routing();

            endpointConfiguration.UseSerialization<JsonSerializer>();
            endpointConfiguration.EnableWindowsPerformanceCounters();
            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.AuditProcessedMessagesTo("audit");

            endpointConfiguration.EnlistWithLegacyMSMQDistributor(
                masterNodeAddress: "Payments.Endpoint.Distributor",
                masterNodeControlAddress: "Payments.Endpoint.Distributor.Distributor.Control",
                capacity: 10);
        }
    }
}
