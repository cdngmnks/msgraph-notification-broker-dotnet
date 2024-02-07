using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace GraphNotificationBroker
{
    public static class MailboxNotificationHandler
    {
        [FunctionName("MailboxNotificationHandler")]
        public static void Run([EventHubTrigger("outlookNotification", Connection = "EventHubConnectionAppSetting")] string myEventHubMessage, ILogger log)
        {
            log.LogInformation($"Function triggered to process a message: {myEventHubMessage}");
        }
    }
}
