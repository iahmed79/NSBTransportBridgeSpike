using System;

namespace Payments.Commands
{
    public class RefundPayment
    {
        public Guid PaymentReference { get; }
        public decimal Amount { get; }

        private RefundPayment() {} // Ensures JSON.NET creates using the non-default constructor

        public RefundPayment(Guid paymentReference, decimal amount)
        {
            PaymentReference = paymentReference;
            Amount = amount;
        }
    }
}
