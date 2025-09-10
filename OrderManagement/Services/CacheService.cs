using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OrderManagement.Services.Contracts;
using StackExchange.Redis;
using System;
using System.Configuration;
using System.Web.Caching;

namespace OrderManagement.Services
{
    public class CacheService : ICacheService
    {
        private static Lazy<ConnectionMultiplexer> lazyConnection;
        private readonly IDatabase _cache;

        public CacheService(IConfiguration configuration)
        {
            var redisConnection = configuration.GetConnectionString("RedisConnection");

            lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
            {
                var configOptions = ConfigurationOptions.Parse(redisConnection);
                configOptions.AbortOnConnectFail = false;

                return ConnectionMultiplexer.Connect(configOptions);
            });

            _cache = Connection.GetDatabase();
        }


        // ✅ Add this constructor for unit testing only
        public CacheService(IDatabase database)
        {
            _cache = database ?? throw new ArgumentNullException(nameof(database));
        }

        private static ConnectionMultiplexer Connection => lazyConnection.Value;

        // Use this property to access the injected or live database
        private IDatabase Cache => _cache;
        //private IDatabase Cache => Connection.GetDatabase();

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