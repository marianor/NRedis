using Framework.Caching;
using Framework.Caching.Protocol;
using Framework.Caching.Transport;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NuGet.Configuration;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace RedisTester
{
    public static class Program
    {
        public async static Task Main(string[] args)
        {
            var loggerFactory = GetLoggerFactory();
            var logger = loggerFactory.CreateLogger("console");

            try
            {
                await DoLocalOperationsAsync(logger).ConfigureAwait(false);
                Console.WriteLine();
                //await DoAzureOperations();
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
        private static async Task DoAzureOperations()
        {
            var settings = new TransportSettings<SslTcpTransport>
            {
                Host = "shadowfly-cache.redis.cache.windows.net",
                Port = 6380,
                Password = "9XovgzlYSDG7Dno2KHMR0TYy2ElkvnnFvdHj591sKjY=",
            };
            await DoOperationsAsync(settings).ConfigureAwait(false);
        }

        private static async Task DoLocalOperationsAsync(ILogger logger)
        {
            var nugetSettings = Settings.LoadDefaultSettings(null);
            var folder = SettingsUtility.GetGlobalPackagesFolder(nugetSettings);

            using (var redisServer = new LocalRedisLauncher(Path.Combine(folder, @"redis-64\3.0.503\tools")))
            {
                var settings = new TransportSettings<TcpTransport>
                {
                    Host = "localhost",
                    Port = 6379,
                    Logger = logger
                };
                await DoOperationsAsync(settings).ConfigureAwait(false);
            }
        }

        private static async Task DoOperationsAsync(ITransportSettings settings)
        {
            var client = new RespClient(settings);

            var cache = new RedisCache(client);
            await cache.SetAsync(string.Empty, Serialize("<Empty Key>")).ConfigureAwait(false);
            await cache.SetAsync("Key", Serialize("Value"), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) }).ConfigureAwait(false);
            await cache.SetAsync("Key2", Serialize("Value2"), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) }).ConfigureAwait(false);

            Console.WriteLine($"Get for 'Key': {Deserialize(await cache.GetAsync("Key").ConfigureAwait(false))}");
            Console.WriteLine($"Get for 'Key2': {Deserialize(await cache.GetAsync("Key2").ConfigureAwait(false))}");
            Console.WriteLine($"Get for 'NoSuchKey': {Deserialize(await cache.GetAsync("NoSuchKey").ConfigureAwait(false)) ?? "<No such key>"}");

            cache.Remove("Key");
            Console.WriteLine($"Value 'Key' deleted: {Deserialize(await cache.GetAsync("Key").ConfigureAwait(false)) ?? "<No such key>"}");
        }

        private static object Deserialize(byte[] serializedValue)
        {
            if (serializedValue == null)
                return null;

            using (var stream = new MemoryStream(serializedValue))
            {
                return new BinaryFormatter().Deserialize(stream);
            }
        }

        private static byte[] Serialize(object value)
        {
            if (value == null)
                return null;

            using (var stream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(stream, value);
                return stream.ToArray();
            }
        }

        private static ILoggerFactory GetLoggerFactory()
        {
            var serviceCollection = new ServiceCollection()
                .AddLogging(builder => builder.AddConsole().AddFilter(l => true))
                .BuildServiceProvider();
            return serviceCollection.GetService<ILoggerFactory>();
        }
    }
}