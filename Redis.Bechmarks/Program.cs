using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Framework.Caching.Redis;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading.Tasks;

namespace Redis.Bechmarks
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<RedisBenchmark>();
        }
    }

    public class RedisBenchmark
    {
        private RedisCache _cache;
        private static readonly Random m_random = new Random();

        public int KeyIndex => m_random.Next();

        [GlobalSetup]
        public void Setup()
        {
            var options = new RedisCacheOptions
            {
                Host = "shadowfly-cache.redis.cache.windows.net",
                Password = "zKkiMoQYp9qgTYvwTecq5LY0Y6b3p74uUNUwPBsQcaM="
            };

            _cache = new RedisCache(options);
        }

        [Benchmark]
        public async Task Set()
        {
            var value = new TestObject { Id = KeyIndex, Name = $"Name {KeyIndex}", Description = $"Description {KeyIndex}" };
            var serializedValue = Serializer.Serialize(value);
            await _cache.SetAsync("Test" + KeyIndex, serializedValue, new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(10) });
        }

        [Benchmark]
        public async Task<TestObject> Get()
        {
            var serializedValue = await _cache.GetAsync("Test" + KeyIndex);
            return Serializer.Deserialize<TestObject>(serializedValue);
        }
    }
}