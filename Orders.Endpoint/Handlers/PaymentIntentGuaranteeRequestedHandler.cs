namespace Orders.Endpoint.Handlers
{
    using System;
    using System.Threading.Tasks;

    using NServiceBus;

    using Payments.Events;

    public class PaymentIntentGuaranteeRequestedHandler : IHandleMessages<IPaymentIntentGuaranteeRequested>
    {
        public Task Handle(IPaymentIntentGuaranteeRequested message, IMessageHandlerContext context)
        {
            Console.WriteLine($"In Orders.Endpoint PaymentIntentGuaranteeRequestedHandler for PaymentReference: {message.PaymentReference}");

            return Task.CompletedTask;
        }
    }
}
