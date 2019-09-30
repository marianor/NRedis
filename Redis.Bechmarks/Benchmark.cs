using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace NRedis.Bechmarks
{
    [CoreJob]
    //[ClrJob] // , MonoJob, CoreRtJob
    [MemoryDiagnoser]
    public class Benchmark
    {
        private RedisCache _cache;
        private byte[] _value;
        private int _keyIndexSync;
        private int _keyIndexAsync = int.MaxValue;
        private DistributedCacheEntryOptions _options;

        public int KeyIndexSync => ++_keyIndexSync;

        public int KeyIndexAsync => --_keyIndexAsync;

        [GlobalSetup]
        public async Task SetupAsync()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var section = configuration.GetSection("Redis");

            var options = new RedisCacheOptions
            {
                Host = section.GetValue<string>("Host"),
                Password = section.GetValue<string>("Password"),
            };

            _value = await Serializer.SerializeAsync(new { Name = "foo", Description = "bar" });
            _options = new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(10) };

            _cache = new RedisCache(options);
        }

        [Benchmark]
        public void Set() => _cache.Set($"Test{KeyIndexSync}", _value, _options);

        [Benchmark]
        public byte[] Get() => _cache.Get($"Test{KeyIndexSync}");

        [Benchmark]
        public async Task SetAsync() => await _cache.SetAsync($"Test{KeyIndexAsync}", _value, _options);

        [Benchmark]
        public async Task<byte[]> GetAsync() => await _cache.GetAsync($"Test{KeyIndexAsync}");
    }
}