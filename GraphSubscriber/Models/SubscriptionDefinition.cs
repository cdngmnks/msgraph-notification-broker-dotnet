using System;

namespace GraphSubscriber.Models
{
    public class SubscriptionDefinition
    {
        public string Resource { get; set; }
        public string ChangeType { get; set; }
        public DateTimeOffset ExpirationDateTime { get; set; }
    }
}
