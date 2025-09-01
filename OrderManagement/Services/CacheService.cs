using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OrderManagement.Services.Contracts;
using StackExchange.Redis;
using System;
using System.Configuration;

namespace OrderManagement.Services
{
    public class CacheService : ICacheService
    {
        private static Lazy<ConnectionMultiplexer> lazyConnection;

        public CacheService(IConfiguration configuration)
        {
            var redisConnection = configuration.GetConnectionString("RedisConnection");

            lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
            {
                var configOptions = ConfigurationOptions.Parse(redisConnection);
                configOptions.AbortOnConnectFail = false;

                try
                {
                    return ConnectionMultiplexer.Connect(configOptions);
                }
                catch (RedisConnectionException ex)
                {
                    CommonUtils.CommonUtils.LogMessage($"Redis connection failed: {ex.Message}");
                    throw;
                }
            });
        }

        private static ConnectionMultiplexer Connection => lazyConnection.Value;

        private IDatabase Cache => Connection.GetDatabase();

        public void Set<T>(string key, T value, TimeSpan expiry)
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                var json = JsonConvert.SerializeObject(value, settings);
                Cache.StringSet(key, json, expiry);
            }
            catch (Exception ex)
            {
                CommonUtils.CommonUtils.LogMessage($"Cache Set error for key when set '{key}': {ex.Message}\n{ex.StackTrace}");
            }
        }

        public T Get<T>(string key)
        {
            try
            {
                var value = Cache.StringGet(key);
                if (!value.HasValue)
                    return default(T);

                return JsonConvert.DeserializeObject<T>(value);
            }
            catch (Exception ex)
            {
                CommonUtils.CommonUtils.LogMessage($"Cache Get error for key when get '{key}': {ex.Message}\n{ex.StackTrace}");
                return default(T);
            }
        }

        public void Remove(string key)
        {
            try
            {
                Cache.KeyDelete(key);
            }
            catch (Exception ex)
            {
                CommonUtils.CommonUtils.LogMessage($"Cache Remove error for key '{key}': {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}