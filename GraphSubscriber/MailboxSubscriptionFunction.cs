using Azure.Identity;
using GraphSubscriber.Models;
using GraphSubscriber.Services;
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

        public MailboxSubscription(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        [FunctionName("RenewMailboxSubscription")]
        public async Task RenewMailboxSubscription([TimerTrigger("0 0 * * * *")] TimerInfo myTimer, ILogger log)
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

            var keys = _cacheService.GetAllKeys();

            foreach( var key in keys) 
            {
                var subscription = await _cacheService.GetAsync<SubscriptionDefinition>(key);

                if (subscription != null)
                {
                    if (subscription.ExpirationDateTime.Date < DateTime.Today.AddDays(1))
                    {
                        await graphClient.Subscriptions[key].PatchAsync(requestBody);
                        await _cacheService.DeleteAsync(key);
                        await _cacheService.AddAsync(key, subscription, TimeSpan.FromDays(3));
                    }
                }
            }            
        }

        [FunctionName("CreateMailboxSubscription")]
        public async Task<IActionResult> CreateMailboxSubscription(
           [HttpTrigger(AuthorizationLevel.Anonymous,"post", Route = null)] HttpRequest req,
           ILogger log)
        {
            var content = await new StreamReader(req.Body).ReadToEndAsync();

            SubscriptionDefinition request = JsonConvert.DeserializeObject<SubscriptionDefinition>(content);

            var scopes = new[] { "https://graph.microsoft.com/.default" };

            var clientSecretCredential = new ClientSecretCredential(TenantId, ClientId, ClientSecret);

            var graphClient = new GraphServiceClient(clientSecretCredential, scopes);

            var requestBody = new Subscription
            {
                ChangeType = request.ChangeType, //"created",
                NotificationUrl = NotificationUrl,
                Resource = request.Resource,
                ExpirationDateTime = request.ExpirationDateTime,
                ClientState = ClientSecret
            };

            var result = await graphClient.Subscriptions.PostAsync(requestBody);

            await _cacheService.AddAsync(result.Id, requestBody, TimeSpan.FromDays(4));

            return new OkResult();
        }

        [FunctionName("DeleteMailboxSubscription")]
        public async Task<IActionResult> DeleteMailboxSubscription(
          [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "DeleteMailboxSubscription/{id}")] HttpRequest req,
          ILogger log, string id)
        {
            var scopes = new[] { "https://graph.microsoft.com/.default" };

            var clientSecretCredential = new ClientSecretCredential(TenantId, ClientId, ClientSecret);

            var graphClient = new GraphServiceClient(clientSecretCredential, scopes);

            var subscription = await _cacheService.GetAsync<SubscriptionDefinition>(id);

            if (subscription != null)
            {
                await graphClient.Subscriptions[id].DeleteAsync();

                await _cacheService.DeleteAsync(id);
            }

            return new OkResult();
        }
    }    
}
