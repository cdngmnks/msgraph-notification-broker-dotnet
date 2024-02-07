using Microsoft.Graph.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GraphSubscriber.Services
{
    public interface ICacheService
    {
        Task<bool> AddAsync<T>(string key, T value, TimeSpan? expiry = default(TimeSpan?));

        Task<T> GetAsync<T>(string key);

        Task<bool> DeleteAsync(string key);

        List<RedisKey> GetAllKeys();
    }
}
