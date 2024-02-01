using Azure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using System;
using System.Threading.Tasks;

namespace GraphNotificationBroker
{
    public static class Notification
    {
        [FunctionName("Notification")]
        public static void Run([EventHubTrigger("outlookNotification", Connection = "EventHubConnectionAppSetting")] string myEventHubMessage, ILogger log)
        {
            log.LogInformation($"Function triggered to process a message: {myEventHubMessage}");
        }
    }

    public static class RenewSubscription
    {
        private static readonly string ClientId = Environment.GetEnvironmentVariable("ClientId");
        private static readonly string ClientSecret = Environment.GetEnvironmentVariable("ClientSecret");
        private static readonly string TenantId = Environment.GetEnvironmentVariable("TenantId");
        private static readonly string NotificationUrl = Environment.GetEnvironmentVariable("NotificationUrl");
        private static readonly string Resource = Environment.GetEnvironmentVariable("Resource");

        [FunctionName("RenewSubscription")]
        public static async Task RunAsync([TimerTrigger("1 59 */2 * * *")] TimerInfo myTimer, ILogger log)
        {
             var clientId = ClientId;
            var clientSecret = ClientSecret;
            var tenantId = TenantId;

            var scopes = new[] { "https://graph.microsoft.com/.default" };

            var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);

            var graphClient = new GraphServiceClient(clientSecretCredential, scopes);

            var requestBody = new Subscription
            {
                ExpirationDateTime = DateTime.UtcNow.AddDays(3),
            };
            var allSubscription = await graphClient.Subscriptions.GetAsync();

            if (allSubscription.Value.Count == 0)
            {
                var body = new Subscription
                {
                    ChangeType = "created",
                    NotificationUrl = NotificationUrl,
                    Resource = Resource,
                    ExpirationDateTime = DateTime.UtcNow.AddDays(3),
                    ClientState = ClientSecret
                };

                await graphClient.Subscriptions.PostAsync(body);
            }
            else
            {
                //this is for demonstration purposes otherwise the subscription id would be fetched from db
                string subscriptionId = allSubscription.Value[0].Id;

                await graphClient.Subscriptions[subscriptionId].PatchAsync(requestBody);
            }
        }
    }

    public static class CreateSubscription
    {
        private static readonly string ClientId = Environment.GetEnvironmentVariable("ClientId");
        private static readonly string ClientSecret = Environment.GetEnvironmentVariable("ClientSecret");
        private static readonly string TenantId = Environment.GetEnvironmentVariable("TenantId");
        private static readonly string NotificationUrl = Environment.GetEnvironmentVariable("NotificationUrl");
        private static readonly string Resource = Environment.GetEnvironmentVariable("Resource");

        [FunctionName("CreateSubscription")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {

            var clientId = ClientId;
            var clientSecret = ClientSecret;
            var tenantId = TenantId;

            var scopes = new[] { "https://graph.microsoft.com/.default" };

            var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);

            var graphClient = new GraphServiceClient(clientSecretCredential, scopes);

            var requestBody = new Subscription
            {
                ChangeType = "created",
                NotificationUrl = NotificationUrl,
                Resource = Resource,
                ExpirationDateTime = DateTime.UtcNow.AddDays(3),
                ClientState = ClientSecret
            };
            var allSubscription = await graphClient.Subscriptions.GetAsync();

            var result = await graphClient.Subscriptions.PostAsync(requestBody);

            return new OkResult();
        }
    }

    public static class DeleteSubscription
    {
        private static readonly string ClientId = Environment.GetEnvironmentVariable("ClientId");
        private static readonly string ClientSecret = Environment.GetEnvironmentVariable("ClientSecret");
        private static readonly string TenantId = Environment.GetEnvironmentVariable("TenantId");

        [FunctionName("DeleteSubscription")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {

            var clientId = ClientId;
            var clientSecret = ClientSecret;
            var tenantId = TenantId;

            var scopes = new[] { "https://graph.microsoft.com/.default" };

            var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);

            var graphClient = new GraphServiceClient(clientSecretCredential, scopes);

            var allSubscription = await graphClient.Subscriptions.GetAsync();

            await graphClient.Subscriptions[allSubscription.Value[0].Id].DeleteAsync();

            return new OkResult();
        }
    }
}
