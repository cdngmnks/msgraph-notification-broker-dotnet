using Azure.Identity;
using GraphSubscriber.Models;
using GraphSubscriber.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GraphSubscriber
{
    public class MailboxSubscription
    {
        private readonly ICacheService _cacheService;
        private static readonly string ClientId = Environment.GetEnvironmentVariable("ClientId");
        private static readonly string ClientSecret = Environment.GetEnvironmentVariable("ClientSecret");
        private static readonly string TenantId = Environment.GetEnvironmentVariable("TenantId");
        private static readonly string NotificationUrl = Environment.GetEnvironmentVariable("NotificationUrl");
        private static readonly string[] scopes = new[] { "https://graph.microsoft.com/.default" };
        private static readonly string LifecycleEventRenewal = "reauthorizationRequired";

        public MailboxSubscription(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        [FunctionName("CreateMailboxSubscription")]
        public async Task<IActionResult> CreateMailboxSubscription(
           [HttpTrigger(AuthorizationLevel.Function,"post", Route = null)] HttpRequest req,
           ILogger log)
        {
            log.LogInformation("Create new subscription.");

            var content = await new StreamReader(req.Body).ReadToEndAsync();

            SubscriptionDefinition request = JsonConvert.DeserializeObject<SubscriptionDefinition>(content);

            var clientSecretCredential = new ClientSecretCredential(TenantId, ClientId, ClientSecret);

            var graphClient = new GraphServiceClient(clientSecretCredential, scopes);

            var requestBody = new Subscription
            {
                ChangeType = request.ChangeType, // the changeType must be "updated" if we want to receive LifecycleNotification
                NotificationUrl = NotificationUrl,
                Resource = request.Resource,
                LifecycleNotificationUrl = request.LifecycleNotificationUrl,
                ExpirationDateTime = request.ExpirationDateTime,
                ClientState = ClientSecret
            };
            
            var result = await graphClient.Subscriptions.PostAsync(requestBody);

            await _cacheService.AddAsync(result.Id, requestBody, TimeSpan.FromDays(7));

            return new OkResult();
        }

        [FunctionName("DeleteMailboxSubscription")]
        public async Task<IActionResult> DeleteMailboxSubscription(
          [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "DeleteMailboxSubscription/{id}")] HttpRequest req,
          ILogger log, string id)
        {
            var clientSecretCredential = new ClientSecretCredential(TenantId, ClientId, ClientSecret);

            var graphClient = new GraphServiceClient(clientSecretCredential, scopes);

            var subscription = await _cacheService.GetAsync<SubscriptionDefinition>(id);

            if (subscription != null)
            {
                await graphClient.Subscriptions[id].DeleteAsync();

                await _cacheService.DeleteAsync(id);
            }
            else
            {
                await graphClient.Subscriptions[id].DeleteAsync();
            }

            return new OkResult();
        }

        [FunctionName("GetAllMailboxSubscription")]
        public async Task<IActionResult> GetAllMailboxSubscription(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
          ILogger log)
        {
            var clientSecretCredential = new ClientSecretCredential(TenantId, ClientId, ClientSecret);

            var graphClient = new GraphServiceClient(clientSecretCredential, scopes);

            var result = await graphClient.Subscriptions.GetAsync();

            return new OkObjectResult(result);
        }

        [FunctionName("QueueHandler")]
        public async Task Run([QueueTrigger("failed-messages", Connection = "QueueConnectionString")] string failedMessagesQueue, ILogger log)
        {
            log.LogInformation($"Queue triggered, message: {failedMessagesQueue}");

            var request = JsonConvert.DeserializeObject<LifecycleNotification>(failedMessagesQueue);

            if (string.IsNullOrEmpty(failedMessagesQueue))
            {
                log.LogWarning("Empty request body.");
            }

            if (request != null)
            {
                foreach (var lifeCycleNotification in request.Value)
                {
                    if (lifeCycleNotification.LifecycleEvent == LifecycleEventRenewal)
                    {
                        var subscriptionId = lifeCycleNotification.SubscriptionId;
                        var clientId = ClientId;
                        var clientSecret = ClientSecret;
                        var tenantId = TenantId;

                        var scopes = new[] { "https://graph.microsoft.com/.default" };

                        var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);

                        var graphClient = new GraphServiceClient(clientSecretCredential, scopes);

                        var expirationDateTime = DateTime.UtcNow.AddDays(3);
                        var newRequest = new Subscription
                        {
                            ExpirationDateTime = expirationDateTime
                        };

                        var subscription = await _cacheService.GetAsync<Subscription>(subscriptionId);

                        if (subscription != null)
                        {
                            var oldExpirationDateTime = subscription.ExpirationDateTime;
                            subscription.ExpirationDateTime = expirationDateTime;

                            if (oldExpirationDateTime > DateTime.UtcNow)
                            {
                                await graphClient.Subscriptions[subscriptionId].PatchAsync(newRequest);
                                await _cacheService.DeleteAsync(subscriptionId);
                                await _cacheService.AddAsync(subscriptionId, subscription, TimeSpan.FromDays(7));
                            }
                            else
                            {
                                var requestBody = new Subscription
                                {
                                    ChangeType = subscription.ChangeType, // the changeType must be "updated" if we want to receive LifecycleNotification
                                    NotificationUrl = NotificationUrl,
                                    Resource = subscription.Resource,
                                    LifecycleNotificationUrl = subscription.LifecycleNotificationUrl,
                                    ExpirationDateTime = subscription.ExpirationDateTime,
                                    ClientState = ClientSecret
                                };

                                var result = await graphClient.Subscriptions.PostAsync(requestBody);
                                await _cacheService.DeleteAsync(subscriptionId);
                                await _cacheService.AddAsync(result.Id, requestBody, TimeSpan.FromDays(7));
                            }
                        }
                    }
                }
            }
        }
    }
}
