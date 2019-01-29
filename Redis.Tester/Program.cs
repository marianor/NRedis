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

namespace Framework.Caching.Redis.Tester
{
    public static class Program
    {
        private static readonly Random m_random = new Random();

        private const string ItemKey = "Item";
        private const string DeletedKey = "Deleted";
        private const string ItemsKey = "ItemArray";
        private const string MissingKey = "Missing";

        public async static Task Main()
        {
            var services = new ServiceCollection()
                .AddLogging(b => b.AddConsole().AddFilter(l => true));

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            services.AddTransient<IDistributedCache, RedisCache>(s =>
            {
                var options = new RedisCacheOptions();
                configuration.GetSection("LocalRedis").Bind(options);
                options.LoggerFactory = s.GetService<ILoggerFactory>();
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

            try
            {
                var caches = provider.GetServices<IDistributedCache>();
                await DoLocalOperationsAsync(caches.First()).ConfigureAwait(false);
                await Console.Out.WriteLineAsync().ConfigureAwait(false);
                await DoOperationsAsync(caches.ElementAt(1)).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                await Console.Out.WriteLineAsync(e.ToString()).ConfigureAwait(false);
            }
            finally
            {
                Console.ReadKey();
            }
        }

        private static async Task DoLocalOperationsAsync(IDistributedCache cache)
        {
            using (var redisServer = new LocalRedisLauncher(Path.Combine(GetNugetFolder(), @"redis-64\3.0.503\tools")))
                await DoOperationsAsync(cache).ConfigureAwait(false);
        }

        private static async Task DoOperationsAsync(IDistributedCache cache)
        {
            await SetAsync(cache, ItemKey, GetData()).ConfigureAwait(false);
            await SetAsync(cache, ItemsKey, GetDataArray(30)).ConfigureAwait(false);
            await SetAsync(cache, DeletedKey, GetData()).ConfigureAwait(false);

            await Console.Out.WriteLineAsync($"Get for '{ItemKey}': {await GetAsync<TestObject>(cache, ItemKey).ConfigureAwait(false)}").ConfigureAwait(false);
            await Console.Out.WriteLineAsync($"Get for '{ItemsKey}': {await GetAsync<TestObject[]>(cache, ItemsKey).ConfigureAwait(false)}").ConfigureAwait(false);
            await Console.Out.WriteLineAsync($"Get for '{DeletedKey}': {await GetAsync<TestObject>(cache, DeletedKey).ConfigureAwait(false)}").ConfigureAwait(false);
            await Console.Out.WriteLineAsync($"Get for '{MissingKey}': {await GetAsync<TestObject>(cache, MissingKey).ConfigureAwait(false)}").ConfigureAwait(false);

            await cache.RefreshAsync(ItemKey).ConfigureAwait(false);
            await cache.RefreshAsync(ItemsKey).ConfigureAwait(false);

            await cache.RemoveAsync(DeletedKey).ConfigureAwait(false);
            await Console.Out.WriteLineAsync($"Value '{DeletedKey}' deleted: {await GetAsync<TestObject>(cache, DeletedKey).ConfigureAwait(false)}").ConfigureAwait(false);
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
            await cache.SetAsync(key, serializedValue, new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(10) }).ConfigureAwait(false);
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