namespace Payments.Events
{
    using System;

    public interface IPaymentIntentGuaranteeRequested
    {
        Guid PaymentReference { get; set; }
    }

    public class PaymentIntentGuaranteeRequested : IPaymentIntentGuaranteeRequested
    {
        public Guid PaymentReference { get; set; }
    }
}
