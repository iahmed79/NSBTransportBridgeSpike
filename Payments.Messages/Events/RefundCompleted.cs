using System;

namespace Payments.Events
{
    public class RefundCompleted : IRefundCompleted
    {
        public Guid PaymentReference { get; }
        public decimal Amount { get; }

        private RefundCompleted() { } // Ensures JSON.NET creates using the non-default constructor

        public RefundCompleted(Guid paymentReference, decimal amount)
        {
            PaymentReference = paymentReference;
            Amount = amount;
        }
    }
}
