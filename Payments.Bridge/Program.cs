namespace Payments.Bridge
{
    using System;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Asos.Finance.Encryption;

    using Newtonsoft.Json;

    using NServiceBus;
    using NServiceBus.Configuration.AdvancedExtensibility;
    using NServiceBus.Router;
    using NServiceBus.Routing;
    using NServiceBus.Serialization;
    using NServiceBus.Settings;
    using NServiceBus.Transport;

    using JsonSerializer = NServiceBus.JsonSerializer;

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

            routerConfig.InterceptForwarding(Intercept);
            
            var staticRouting = routerConfig.UseStaticRoutingProtocol();
            staticRouting.AddForwardRoute("Left", "Right");
            staticRouting.AddForwardRoute("Right", "Left");

            var router = Router.Create(routerConfig);
            
            await router.Start();
            
            Console.ReadLine();
        }

        private static Task Intercept(string inputqueue, MessageContext message, Dispatch dispatchmethod, Func<Dispatch, Task> forwardmethod)
        {
            return forwardmethod((ops, transaction, context) =>
                {
                    var key = "gdDbqRpqdRbTs3mhdZh9qCaDaxJXl+e6";
                    var replacedBody = message.Body;

                    if (ops.MulticastTransportOperations.Any())
                    {
                        var op = ops.MulticastTransportOperations.Single();

                        if (!op.Message.Headers.ContainsKey(Headers.EnclosedMessageTypes))
                        {
                            return dispatchmethod(ops, transaction, context);
                        }

                        if (op.Message.Body.Length > 0 && inputqueue == "Left")
                        {
                            Console.WriteLine("encrypt here");
                            var encrypted = new Encrypter().Encrypt(key, op.Message.Body);
                            replacedBody = encrypted.Payload;
                            op.Message.Headers.Add("encryption-key-iv", encrypted.InitialisationVector);
                        }

                        if (op.Message.Body.Length > 0 && inputqueue == "Right")
                        {
                            var iv = op.Message.Headers["encryption-key-iv"];

                            Console.WriteLine("decrypt this:");
                            Console.WriteLine(Encoding.UTF8.GetString(op.Message.Body));
                            var encryptedPayload = new EncryptedPayload(op.Message.Body, iv, key);
                            replacedBody = Encoding.UTF8.GetBytes(new Encrypter().Decrypt(encryptedPayload));
                            Console.WriteLine();
                            Console.WriteLine(Encoding.UTF8.GetString(replacedBody));
                        }

                        var newMessage = new OutgoingMessage(op.Message.MessageId, op.Message.Headers, replacedBody);
                        var newOp = new TransportOperation(
                            newMessage,
                            new MulticastAddressTag(op.MessageType),
                            op.RequiredDispatchConsistency,
                            op.DeliveryConstraints);
                        var newOps = new TransportOperations(newOp);

                        return dispatchmethod(newOps, transaction, context);
                    }


                    if (ops.UnicastTransportOperations.Any())
                    {
                        var op = ops.UnicastTransportOperations.Single();
                        
                        if (!op.Message.Headers.ContainsKey(Headers.EnclosedMessageTypes))
                        {
                            return dispatchmethod(ops, transaction, context);
                        }

                        if (op.Message.Body.Length > 0 && inputqueue == "Left")
                        {
                            Console.WriteLine("encrypt here");
                            var encrypted = new Encrypter().Encrypt(key, op.Message.Body);
                            replacedBody = encrypted.Payload;
                            op.Message.Headers.Add("encryption-key-iv", encrypted.InitialisationVector);
                        }

                        if (op.Message.Body.Length > 0 && inputqueue == "Right")
                        {
                            var iv = op.Message.Headers["encryption-key-iv"];

                            Console.WriteLine("decrypt this:");
                            Console.WriteLine(Encoding.UTF8.GetString(op.Message.Body));
                            var encryptedPayload = new EncryptedPayload(op.Message.Body, iv, key);
                            replacedBody = Encoding.UTF8.GetBytes(new Encrypter().Decrypt(encryptedPayload));
                            Console.WriteLine();
                            Console.WriteLine(Encoding.UTF8.GetString(replacedBody));
                        }

                        var newMessage = new OutgoingMessage(op.Message.MessageId, op.Message.Headers, replacedBody);
                        var newOp = new TransportOperation(
                            newMessage,
                            new UnicastAddressTag(op.Destination),
                            op.RequiredDispatchConsistency,
                            op.DeliveryConstraints);
                        var newOps = new TransportOperations(newOp);

                        return dispatchmethod(newOps, transaction, context);
                    }

                    return dispatchmethod(ops, transaction, context);
                });
        }
    }
}
