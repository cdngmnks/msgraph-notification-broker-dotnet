using StackExchange.Redis;
using System.Collections.Generic;

namespace GraphSubscriber.Services
{
    public interface IRedisFactory
    {
        IDatabase GetCache();
        List<RedisKey> GetServerKeys();
        void ForceReconnect();
    }
}
