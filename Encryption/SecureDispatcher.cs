namespace Encryption
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Asos.Finance.Encryption;

    using NServiceBus;
    using NServiceBus.Extensibility;
    using NServiceBus.Router;
    using NServiceBus.Routing;
    using NServiceBus.Transport;

    public static class SecureDispatcher
    {
        private const string Key = "gdDbqRpqdRbTs3mhdZh9qCaDaxJXl+e6";

        public static bool ContainsEnclosedMessageTypes(TransportOperations ops)
        {
            var op = GetOperation(ops);
            return op.Message.Headers.ContainsKey(Headers.EnclosedMessageTypes);
        }

        public static Task DispatchWithEncryptedMessageBody(
            TransportOperations ops,
            Dispatch dispatchmethod,
            TransportTransaction transaction,
            ContextBag context)
        {
            var op = GetOperation(ops);
            var newMessage = EncryptMessageBody(op);
            return DispatchWithNewMessage(op, dispatchmethod, transaction, context, newMessage);
        }

        public static Task DispatchWithDecryptedMessageBody(
            TransportOperations ops,
            Dispatch dispatchmethod,
            TransportTransaction transaction,
            ContextBag context)
        {
            var op = GetOperation(ops);
            var newMessage = DecryptMessageBody(op);
            return DispatchWithNewMessage(op, dispatchmethod, transaction, context, newMessage);
        }

        private static IOutgoingTransportOperation GetOperation(TransportOperations ops)
        {
            if (ops.UnicastTransportOperations.Any())
            {
                return ops.UnicastTransportOperations.Single();
            }

            return ops.MulticastTransportOperations.Single();
        }

        private static Task DispatchWithNewMessage(
            IOutgoingTransportOperation op,
            Dispatch dispatchmethod,
            TransportTransaction transaction,
            ContextBag context,
            OutgoingMessage newMessage)
        {
            var addressTag = GetAddressTag(op);

            var newOp = new TransportOperation(
                newMessage,
                addressTag,
                op.RequiredDispatchConsistency,
                op.DeliveryConstraints);
            var newOps = new TransportOperations(newOp);

            return dispatchmethod(newOps, transaction, context);
        }

        private static AddressTag GetAddressTag(IOutgoingTransportOperation op)
        {
            AddressTag addressTag;
            if (op.GetType() == typeof(UnicastTransportOperation))
            {
                var unicastOperation = (UnicastTransportOperation)op;
                addressTag = new UnicastAddressTag(unicastOperation.Destination);
            }
            else
            {
                var multicastOperation = (MulticastTransportOperation)op;
                addressTag = new MulticastAddressTag(multicastOperation.MessageType);
            }

            return addressTag;
        }

        private static OutgoingMessage EncryptMessageBody(IOutgoingTransportOperation op)
        {
            var replacedBody = op.Message.Body;

            if (op.Message.Body.Length > 0)
            {
                Console.WriteLine("encrypt here");
                var encrypted = new Encrypter().Encrypt(Key, op.Message.Body);
                replacedBody = encrypted.Payload;
                op.Message.Headers.Add("encryption-key-iv", encrypted.InitialisationVector);
            }

            return new OutgoingMessage(op.Message.MessageId, op.Message.Headers, replacedBody);
        }

        private static OutgoingMessage DecryptMessageBody(IOutgoingTransportOperation op)
        {
            var replacedBody = op.Message.Body;

            if (op.Message.Body.Length > 0)
            {
                var iv = op.Message.Headers["encryption-key-iv"];

                Console.WriteLine("decrypt this:");
                Console.WriteLine(Encoding.UTF8.GetString(op.Message.Body));
                var encryptedPayload = new EncryptedPayload(op.Message.Body, iv, Key);
                replacedBody = Encoding.UTF8.GetBytes(new Encrypter().Decrypt(encryptedPayload));
                Console.WriteLine();
                Console.WriteLine(Encoding.UTF8.GetString(replacedBody));
            }

            return new OutgoingMessage(op.Message.MessageId, op.Message.Headers, replacedBody);
        }
    }
}
