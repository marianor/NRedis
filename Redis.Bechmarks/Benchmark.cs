using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading.Tasks;

namespace Framework.Caching.Redis.Bechmarks
{
    [CoreJob/*, ClrJob , MonoJob, CoreRtJob*/]
    public class Benchmark
    {
        private RedisCache _cache;
        private byte[] _value;
        private int m_random;
        private DistributedCacheEntryOptions _options;

        public int KeyIndex => ++m_random;

        [GlobalSetup]
        public void Setup()
        {
            var options = new RedisCacheOptions
            {
                Host = "shadowfly-cache.redis.cache.windows.net",
                Password = "zKkiMoQYp9qgTYvwTecq5LY0Y6b3p74uUNUwPBsQcaM="
            };

            var value = new { Name = "foo", Description = "bar" };
            _value = Serializer.Serialize(value);
            _options = new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(10) };

            _cache = new RedisCache(options);
        }

        [Benchmark]
        public void Set()
        {
            _cache.Set("Test" + KeyIndex + 1, _value, _options);
        }

        [Benchmark]
        public byte[] Get()
        {
            return _cache.Get("Test" + KeyIndex + 1);
        }

        [Benchmark]
        public async Task SetAsync()
        {
            await _cache.SetAsync("Test" + KeyIndex, _value, _options);
        }

        [Benchmark]
        public async Task<byte[]> GetAsync()
        {
            return await _cache.GetAsync("Test" + KeyIndex);
        }
    }
}