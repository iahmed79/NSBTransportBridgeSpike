namespace Payments.Messages.Events
{
    using System;

    public interface IRefundCompleted
    {
        decimal Amount { get; }

        Guid PaymentReference { get; }
    }
}