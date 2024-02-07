
using GraphSubscriber.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading;

[assembly: FunctionsStartup(typeof(GraphSubscriber.Startup))]

namespace GraphSubscriber
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // set min threads to reduce chance for Redis timeoutes
            // https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/cache-troubleshoot-client#traffic-burst
            ThreadPool.SetMinThreads(200, 200);

            builder.Services.AddSingleton<IRedisFactory, RedisFactory>();
            builder.Services.AddSingleton<ICacheService>(x =>
                new CacheService(
                    x.GetRequiredService<IRedisFactory>(),
                    x.GetRequiredService<ILogger<CacheService>>()));
        }
    }
}
