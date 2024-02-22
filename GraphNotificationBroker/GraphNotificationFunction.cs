using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.Storage;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System;
using Azure.Messaging.EventHubs;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Net.Http.Json;
using Microsoft.Azure.Amqp.Framing;
using System.Collections;

namespace GraphNotificationBroker
{
    public static class MailboxNotificationHandler
    {
        private static readonly string LifecycleEventRenewal = "reauthorizationRequired";

        [FunctionName("MailboxNotificationHandler")]
        public static async Task Run([EventHubTrigger("outlookNotification", Connection = "EventHubConnectionAppSetting")] string myEventHubMessage,
            [Queue("failed-messages", Connection = "QueueConnectionString")] IAsyncCollector<string> failedMessagesQueue,
            ILogger log)
        {
            log.LogInformation($"Function triggered to process a message: {myEventHubMessage}");
            
            if (myEventHubMessage.Contains(LifecycleEventRenewal))
            {
                await failedMessagesQueue.AddAsync(myEventHubMessage);
            }
        }       
    }
}
