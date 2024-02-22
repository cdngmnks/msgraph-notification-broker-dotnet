using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphSubscriber.Models
{
    public class LifecycleNotification
    {
        public NotificationValue[] Value { get; set; }
    }

    public class NotificationValue
    {
        public string LifecycleEvent { get; set; }
        public string SubscriptionId { get; set; }
        public string Resource { get; set; }
        public string ClientState { get; set; }
        public object Sequence { get; set; } // Nullable
        public ResourceData ResourceData { get; set; }
    }

    public class ResourceData
    {
        public string OdataType { get; set; }
        public string OdataId { get; set; }
        public string Id { get; set; }
        public object EncryptedContent { get; set; } // Nullable
        public string OrganizationId { get; set; }
        public DateTime SubscriptionExpirationDateTime { get; set; }
    }
}
