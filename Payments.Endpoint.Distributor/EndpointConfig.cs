using System;
using NServiceBus.Logging;
using NServiceBus.Persistence;

namespace Payments.Endpoint.Distributor
{
    using NServiceBus;

    public class EndpointConfig : IConfigureThisEndpoint, AsA_Server
    {
        public void Customize(BusConfiguration configuration)
        {
            var dbConnectionString = Environment.GetEnvironmentVariable("NSBCompat_DBConnectionString");

            LogManager.Use<DefaultFactory>().Level(LogLevel.Debug);
            configuration.UseSerialization<JsonSerializer>();
            configuration.UsePersistence<NHibernatePersistence>().ConnectionString(dbConnectionString); // NOTE: Persistence still required because of timeouts etc.
            configuration.RunMSMQDistributor(withWorker: false); // NSB by default sets the endpoint to be a worker too
        }
    }
}
