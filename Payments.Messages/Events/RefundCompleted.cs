namespace Payments.Messages.Events
{
    using System;

    public class RefundCompleted : IRefundCompleted
    {
        public Guid PaymentReference { get; }

        public decimal Amount { get; }

        private RefundCompleted() { } // Ensures JSON.NET creates using the non-default constructor

        public RefundCompleted(Guid paymentReference, decimal amount)
        {
            this.PaymentReference = paymentReference;
            this.Amount = amount;
        }
    }
}
