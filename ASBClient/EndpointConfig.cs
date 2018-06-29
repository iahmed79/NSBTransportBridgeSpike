namespace ASBClient
{
    using System;
    using System.Threading.Tasks;
    using Encryption;
    using NServiceBus;
    using NServiceBus.MessageMutator;
    using Payments.Messages.Commands;
    using Payments.Messages.Events;

    public class EndpointConfig : IConfigureThisEndpoint
    {
        public void Customize(EndpointConfiguration endpointConfiguration)
        {
            var asbConnectionString = Environment.GetEnvironmentVariable("NSBCompat_ASBConnectionString");
            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
            endpointConfiguration.AddDeserializer<XmlSerializer>();
            endpointConfiguration.RegisterMessageMutator(new MutateOutgoingTransportMessages());

            var transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();
            transport.ConnectionString(asbConnectionString);
            transport.UseForwardingTopology();

            var routing = transport.Routing();
            var bridge = routing.ConnectToRouter("Payments.Bridge");

            var conventions = endpointConfiguration.Conventions();
            conventions.DefiningCommandsAs(t => t.Namespace != null && t.Namespace.EndsWith("Commands"));
            conventions.DefiningEventsAs(t => t.Namespace != null && t.Namespace.EndsWith("Events"));

            bridge.RouteToEndpoint(typeof(RefundPayment), "Payments.Endpoint.Distributor");
        }
    }

    public class MessageSender : IWantToRunWhenEndpointStartsAndStops
    {
        public Task Start(IMessageSession session)
        {
            Console.WriteLine("Press C to send a command");
            Console.WriteLine("Press E to send an event");
            Console.WriteLine("Press any other key to exit");

            while (true)
            {
                var key = Console.ReadKey();

                if (key.Key != ConsoleKey.C && key.Key != ConsoleKey.E)
                {
                    break;
                }

                if (key.Key == ConsoleKey.C)
                {
                    session.Send(new RefundPayment(Guid.NewGuid(), 100m));
                    Console.WriteLine("\nCommand sent");
                }

                if (key.Key == ConsoleKey.E)
                {
                    session.Publish<IPaymentIntentGuaranteeRequested>(@event => { @event.PaymentReference = Guid.NewGuid(); });
                    Console.WriteLine("\nEvent published");
                }
            }

            return Task.CompletedTask;
        }

        public Task Stop(IMessageSession session)
        {
            return Task.CompletedTask;
        }
    }
}
