namespace Payments.Endpoint.Worker.Handlers
{
    using System;
    using System.Threading.Tasks;

    using NServiceBus;

    using Payments.Messages.Commands;
    using Payments.Messages.Events;

    public class RefundPaymentHandler : IHandleMessages<RefundPayment>
    {
        public async Task Handle(RefundPayment message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Refunded {message.Amount} on payment {message.PaymentReference}");

            await context.Publish(new RefundCompleted(message.PaymentReference, message.Amount));
        }
    }
}
