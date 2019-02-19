using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NuGet.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NRedis.Tester
{
    public static class Program
    {
        private static readonly Random m_random = new Random();

        private const string ItemKey = "Item";
        private const string DeletedKey = "Deleted";
        private const string ItemsKey = "Items";
        private const string MissingKey = "Missing";

        public async static Task Main()
        {
            var services = new ServiceCollection()
                .AddLogging(b => b.AddConsole().AddFilter(l => true));

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            services.AddTransient<IDistributedCache, RedisCache>(prov =>
            {
                var options = new RedisCacheOptions();
                configuration.GetSection("LocalRedis").Bind(options);
                options.LoggerFactory = prov.GetService<ILoggerFactory>();
                return new RedisCache(options);
            });

            services.AddTransient<IDistributedCache, RedisCache>(s =>
            {
                var options = new RedisCacheOptions();
                configuration.GetSection("AzureRedis").Bind(options);
                options.LoggerFactory = s.GetService<ILoggerFactory>();
                return new RedisCache(options);
            });

            var provider = services.BuildServiceProvider();

            var loggerFactory = provider.GetService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(typeof(Program));

            try
            {
                var caches = provider.GetServices<IDistributedCache>();
                if (configuration.GetValue<bool>("LocalRedis:Enabled"))
                    await DoLocalOperationsAsync(caches.First(), logger).ConfigureAwait(false);

                if (configuration.GetValue<bool>("AzureRedis:Enabled"))
                    await DoOperationsAsync(caches.ElementAt(1), logger).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
            }
        }

        private static async Task DoLocalOperationsAsync(IDistributedCache cache, ILogger logger)
        {
            using (var redisServer = new LocalRedisLauncher(Path.Combine(GetNugetFolder(), @"redis-64\3.0.503\tools")))
                await DoOperationsAsync(cache, logger).ConfigureAwait(false);
        }

        private static async Task DoOperationsAsync(IDistributedCache cache, ILogger logger)
        {
            await cache.SetAsync(ItemKey, GetData(), logger).ConfigureAwait(false);
            await cache.SetAsync(ItemsKey, GetDataArray(30), logger).ConfigureAwait(false);
            await cache.SetAsync(DeletedKey, GetData(), logger).ConfigureAwait(false);

            await cache.GetAsync<TestObject>(ItemKey, logger).ConfigureAwait(false);
            await cache.GetAsync<TestObject[]>(ItemsKey, logger).ConfigureAwait(false);
            await cache.GetAsync<TestObject>(DeletedKey, logger).ConfigureAwait(false);
            await cache.GetAsync<TestObject>(MissingKey, logger).ConfigureAwait(false);

            await cache.RefreshAsync(ItemKey).ConfigureAwait(false);
            await cache.RefreshAsync(ItemsKey).ConfigureAwait(false);

            await cache.RemoveAsync(DeletedKey, logger).ConfigureAwait(false);
            await cache.GetAsync<TestObject>(DeletedKey, logger).ConfigureAwait(false);
        }

        private static async Task<T> GetAsync<T>(this IDistributedCache cache, string key, ILogger logger)
        {
            var buffer = await cache.GetAsync(key).ConfigureAwait(false);
            var value = Serializer.Deserialize<T>(buffer);
            if (value == null)
            {
                logger.LogInformation($"Get for '{key}': <Missing Key>");
                return default;
            }

            var textValue = JsonConvert.SerializeObject(value);
            logger.LogInformation($"Get for '{key}': {textValue?.Substring(0, Math.Min(textValue.Length, 97))}");
            return value;
        }

        private static async Task SetAsync(this IDistributedCache cache, string key, object value, ILogger logger)
        {
            var serializedValue = Serializer.Serialize(value);
            await cache.SetAsync(key, serializedValue, new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(10) }).ConfigureAwait(false);
            logger.LogInformation($"Item with key '{key}' set");
        }

        private static async Task RemoveAsync(this IDistributedCache cache, string key, ILogger logger)
        {
            await cache.RemoveAsync(key).ConfigureAwait(false);
            logger.LogInformation($"Item with key '{key}' removed");
        }

        private static string GetNugetFolder()
        {
            var nugetSettings = Settings.LoadDefaultSettings(null);
            return SettingsUtility.GetGlobalPackagesFolder(nugetSettings);
        }

        private static TestObject GetData()
        {
            return new TestObject
            {
                Id = m_random.Next(),
                Name = $"Name {m_random.Next()}",
                Description = $"Description {m_random.Next()}"
            };
        }

        private static TestObject[] GetDataArray(int length)
        {
            return Enumerable.Range(1, length).Select(i => GetData()).ToArray();
        }
    }
}