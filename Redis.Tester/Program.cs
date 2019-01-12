using Framework.Caching.Redis;
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

namespace Redis.Tester
{
    public static class Program
    {
        private static readonly Random m_random = new Random();

        private const string ItemKey = "Item";
        private const string ItemsKey = "ItemArray";
        private const string MissingKey = "Missing";

        public async static Task Main()
        {
            var services = new ServiceCollection()
                .AddLogging(b => b.AddConsole()/*.AddFilter(l => true)*/);

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            services.AddTransient<IDistributedCache, RedisCache>(x =>
            {
                var options = new RedisCacheOptions();
                configuration.GetSection("LocalRedis").Bind(options);
                return new RedisCache(options);
            });

            services.AddTransient<IDistributedCache, RedisCache>(x =>
            {
                var options = new RedisCacheOptions();
                configuration.GetSection("AzureRedis").Bind(options);
                return new RedisCache(options);
            });

            var provider = services.BuildServiceProvider();

            var loggerFactory = provider.GetService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("console");

            try
            {
                var caches = provider.GetServices<IDistributedCache>();
                await DoLocalOperationsAsync(caches.ElementAt(0)).ConfigureAwait(false);
                Console.WriteLine();
                await DoAzureOperationsAsync(caches.ElementAt(1)).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                Console.ReadKey();
            }
        }
        private static async Task DoAzureOperationsAsync(IDistributedCache cache)
        {
            await DoOperationsAsync(cache).ConfigureAwait(false);
        }

        private static async Task DoLocalOperationsAsync(IDistributedCache cache)
        {
            using (var redisServer = new LocalRedisLauncher(Path.Combine(GetNugetFolder(), @"redis-64\3.0.503\tools")))
                await DoOperationsAsync(cache).ConfigureAwait(false);
        }

        private static async Task DoOperationsAsync(IDistributedCache cache)
        {
            await SetAsync(cache, ItemKey, GetData()).ConfigureAwait(false);
            await SetAsync(cache, ItemsKey, GetDataArray(10)).ConfigureAwait(false);

            Console.WriteLine($"Get for '{ItemKey}': {await GetAsync<TestObject>(cache, ItemKey).ConfigureAwait(false)}");
            Console.WriteLine($"Get for '{ItemsKey}': {await GetAsync<TestObject[]>(cache, ItemsKey).ConfigureAwait(false)}");
            Console.WriteLine($"Get for '{MissingKey}': {await GetAsync<TestObject>(cache, MissingKey).ConfigureAwait(false)}");

            cache.Remove(ItemKey);
            Console.WriteLine($"Value '{ItemKey}' deleted: {await GetAsync<TestObject>(cache, ItemKey).ConfigureAwait(false)}");
        }

        private static async Task<string> GetAsync<T>(IDistributedCache cache, string key)
        {
            var buffer = await cache.GetAsync(key).ConfigureAwait(false);
            var value = Serializer.Deserialize<T>(buffer);
            if (value == null)
                return "<Missing Key>";

            var textValue = JsonConvert.SerializeObject(value);
            return textValue?.Substring(0, Math.Min(textValue.Length, 98));
        }

        private static async Task SetAsync(IDistributedCache cache, string key, object value)
        {
            var serializedValue = Serializer.Serialize(value);
            await cache.SetAsync(key, serializedValue, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) }).ConfigureAwait(false);
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