namespace ASBClient
{
    using System;
    using System.Text;
    using System.Threading.Tasks;

    using Asos.Finance.Encryption;

    using NServiceBus.MessageMutator;

    public class MutateOutgoingTransportMessages :
        IMutateOutgoingTransportMessages
    {
        public Task MutateOutgoing(MutateOutgoingTransportMessageContext context)
        {
            Console.WriteLine("Mutating:");
            Console.WriteLine(Encoding.UTF8.GetString(context.OutgoingBody));

            var key = "gdDbqRpqdRbTs3mhdZh9qCaDaxJXl+e6";
            var encrypted = new Encrypter().Encrypt(key, context.OutgoingBody);
            context.OutgoingBody = encrypted.Payload;
            context.OutgoingHeaders.Add("encryption-key-iv", encrypted.InitialisationVector);

            Console.WriteLine(Encoding.UTF8.GetString(context.OutgoingBody));
            return Task.CompletedTask;
        }
    }
}
