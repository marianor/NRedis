using Framework.Caching;
using Framework.Caching.Protocol;
using Framework.Caching.Transport;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
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
        public async static Task Main()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var serviceCollection = new ServiceCollection()
                .AddLogging(builder => builder.AddConsole().AddFilter(l => true))
                .BuildServiceProvider();

            var loggerFactory = serviceCollection.GetService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("console");

            try
            {
                await DoLocalOperationsAsync(configuration, logger).ConfigureAwait(false);
                Console.WriteLine();
                await DoAzureOperationsAsync(configuration, logger).ConfigureAwait(false);
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
        private static async Task DoAzureOperationsAsync(IConfiguration configuration, ILogger logger)
        {
            var settings = new TransportSettings<SslTcpTransport>
            {
                Host = configuration.GetValue<string>("AzureRedis:Host"),
                Port = configuration.GetValue<int>("AzureRedis:Port"),
                Password = configuration.GetValue<string>("AzureRedis:Password"),
                Logger = logger
            };
            await DoOperationsAsync(settings).ConfigureAwait(false);
        }

        private static async Task DoLocalOperationsAsync(IConfiguration configuration, ILogger logger)
        {
            var nugetSettings = Settings.LoadDefaultSettings(null);
            var folder = SettingsUtility.GetGlobalPackagesFolder(nugetSettings);

            using (var redisServer = new LocalRedisLauncher(Path.Combine(folder, @"redis-64\3.0.503\tools")))
            {
                var settings = new TransportSettings<TcpTransport>
                {
                    Host = configuration.GetValue<string>("LocalRedis:Host"),
                    Port = configuration.GetValue<int>("LocalRedis:Port"),
                    Logger = logger
                };
                await DoOperationsAsync(settings).ConfigureAwait(false);
            }
        }

        private static async Task DoOperationsAsync(ITransportSettings settings)
        {
            var client = new RespClient(settings);

            var cache = new RedisCache(client);
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
    }
}