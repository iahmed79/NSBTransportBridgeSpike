namespace Payments.Endpoint.Worker.Handlers
{
    using System;
    using System.Threading.Tasks;

    using NServiceBus;

    using Payments.Messages.Events;

    public class PaymentIntentGuaranteeRequestedHandler : IHandleMessages<IPaymentIntentGuaranteeRequested>
    {
        public Task Handle(IPaymentIntentGuaranteeRequested message, IMessageHandlerContext context)
        {
            Console.WriteLine($"IPaymentIntentGuaranteeRequested on payment {message.PaymentReference}");

            return Task.CompletedTask;
        }
    }
}
