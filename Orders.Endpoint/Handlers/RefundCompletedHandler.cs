using System;
using System.Threading.Tasks;
using NServiceBus;

namespace Orders.Endpoint.Handlers
{
    using Payments.Events;

    public class RefundCompletedHandler : IHandleMessages<IRefundCompleted>
    {
        public Task Handle(IRefundCompleted message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Refund completed for payment {message.PaymentReference}");
            return Task.CompletedTask;
        }
    }
}
