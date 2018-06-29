using System;

namespace Orders.Endpoint
{
    using NServiceBus;
    using NServiceBus.MessageMutator;
    using Payments.Messages.Events;

    public class EndpointConfig : IConfigureThisEndpoint
    {
        public void Customize(EndpointConfiguration endpointConfiguration)
        {
            var dbConnectionString = Environment.GetEnvironmentVariable("NSBCompat_DBConnectionString");
            var asbConnectionString = Environment.GetEnvironmentVariable("NSBCompat_ASBConnectionString");
            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
            endpointConfiguration.AddDeserializer<XmlSerializer>();
            endpointConfiguration.RegisterMessageMutator(new MutateOutgoingTransportMessages());

            var transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();
            transport.ConnectionString(asbConnectionString);
            
            var topology = transport.UseForwardingTopology();

            var conventions = endpointConfiguration.Conventions();
            conventions.DefiningEventsAs(t => t.Namespace != null && t.Namespace.EndsWith("Events"));

            var bridge = transport.Routing().ConnectToRouter("Payments.Bridge");
            bridge.RegisterPublisher(typeof(IRefundCompleted), "payments.endpoint.distributor");

            //TODO: NServiceBus provides multiple durable storage options, including SQL Server, RavenDB, and Azure Storage Persistence.
            // Refer to the documentation for more details on specific options.
            //endpointConfiguration.UsePersistence<PLEASE_SELECT_ONE>();

            // NServiceBus will move messages that fail repeatedly to a separate "error" queue. We recommend
            // that you start with a shared error queue for all your endpoints for easy integration with ServiceControl.
            endpointConfiguration.SendFailedMessagesTo("error");

            // NServiceBus will store a copy of each successfully process message in a separate "audit" queue. We recommend
            // that you start with a shared audit queue for all your endpoints for easy integration with ServiceControl.
            endpointConfiguration.AuditProcessedMessagesTo("audit");
        }
    }
}
