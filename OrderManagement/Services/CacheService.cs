using Newtonsoft.Json;
using OrderManagement.Services.Contracts;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace OrderManagement.Services
{
    public class CacheService : ICacheService
    {
        private static readonly Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            // TODO: Replace with your Redis connection string
            string redisConnection = ConfigurationManager.AppSettings["RedisConnection"];
            return ConnectionMultiplexer.Connect(redisConnection);// ("localhost:5000");
        });

        private static ConnectionMultiplexer Connection => lazyConnection.Value;

        private IDatabase Cache => Connection.GetDatabase();

        public void Set<T>(string key, T value, TimeSpan expiry)
        {
            var json = JsonConvert.SerializeObject(value);
            Cache.StringSet(key, json, expiry);
        }

        public T Get<T>(string key)
        {
            var value = Cache.StringGet(key);
            if (!value.HasValue)
                return default(T);
            return JsonConvert.DeserializeObject<T>(value);
        }

        public void Remove(string key)
        {
            Cache.KeyDelete(key);
        }
    }
}