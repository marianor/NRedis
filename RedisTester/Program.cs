using Framework.Caching;
using Framework.Caching.Protocol;
using Framework.Caching.Transport;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace RedisTester
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            try
            {
                //,password=,ssl=True,abortConnect=False
                //await DoLocalOperations();
                Console.WriteLine();
                await DoAzureOperations();
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
            await DoOperations(settings);
        }

        private static async Task DoLocalOperations()
        {
            using (var redisServer = new LocalRedisLauncher(@"C:\Users\maro\.nuget\packages\redis-64\3.0.503\tools"))
            {
                var settings = new TransportSettings<TcpTransport>
                {
                    Host = "localhost",
                    Port = 6379
                };
                await DoOperations(settings);
            }
        }

        private static async Task DoOperations(ITransportSettings settings)
        {
            //var transport = new TcpTransport("shadowfly-cache.redis.cache.windows.net", 6380);
            //transport.Logger = new ConsoleLogger("Console", (m, l) => l == LogLevel.Information, true);

            var client = new RespClient(settings);

            var cache = new RedisCache(client);
            await cache.SetAsync("Key", Serialize("Value"), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
            await cache.SetAsync("Key2", Serialize("Value2"), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });

            Console.WriteLine($"Get for 'Key': {Deserialize(await cache.GetAsync("Key"))}");
            Console.WriteLine($"Get for 'Key2': {Deserialize(await cache.GetAsync("Key2"))}");
            Console.WriteLine($"Get for 'NoSuchKey': {Deserialize(await cache.GetAsync("NoSuchKey")) ?? "<No such key>"}");

            cache.Remove("Key");
            Console.WriteLine($"Value 'Key' deleted: {Deserialize(await cache.GetAsync("Key")) ?? "<No such key>"}");
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