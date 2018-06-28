namespace Payments.Messages.Commands
{
    using System;

    public class RefundPayment
    {
        public Guid PaymentReference { get; }
        public decimal Amount { get; }

        private RefundPayment() {} // Ensures JSON.NET creates using the non-default constructor

        public RefundPayment(Guid paymentReference, decimal amount)
        {
            this.PaymentReference = paymentReference;
            this.Amount = amount;
        }
    }
}
