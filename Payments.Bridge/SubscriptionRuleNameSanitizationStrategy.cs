namespace Payments.Bridge
{
    using System.Linq;

    using NServiceBus;
    using NServiceBus.Transport.AzureServiceBus;

    internal class SubscriptionRuleNameSanitizationStrategy : ISanitizationStrategy
    {
        public string Sanitize(string entityPathOrName, EntityType entityType)
        {
            return entityType == EntityType.Rule ? entityPathOrName.Split('.').Last() : entityPathOrName;
        }
    }
}