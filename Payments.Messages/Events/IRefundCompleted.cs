using System;

namespace Payments.Events
{
    public interface IRefundCompleted
    {
        decimal Amount { get; }
        Guid PaymentReference { get; }
    }
}