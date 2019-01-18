using Framework.Caching.Redis.Properties;
using Framework.Caching.Redis.Protocol;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Caching.Redis
{
    public class RedisCache : IDistributedCache
    {
        private readonly IRespClient _respClient;

        public RedisCache(IOptions<RedisCacheOptions> optionsAccessor, IRespClient client = null)
        {
            if (optionsAccessor == null)
                throw new ArgumentNullException(nameof(optionsAccessor));

            _respClient = client ?? new RespClient(optionsAccessor);
        }

        public byte[] Get(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (key.Length == 0)
                throw new ArgumentException(Resources.ArgumentCannotBeEmpty, nameof(key));

            var request = new Request(CommandType.Get, key);
            var response = _respClient.Execute(request);
            return response.GetRawValue();
        }

        /// <summary>
        /// Takes a string key and retrieves a cached item as a byte[] if found in the cache.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (key.Length == 0)
                throw new ArgumentException(Resources.ArgumentCannotBeEmpty, nameof(key));

            var request = new Request(CommandType.Get, key);
            var response = await _respClient.ExecuteAsync(request, token).ConfigureAwait(false);
            return response.GetRawValue();
        }

        public void Refresh(string key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Refreshes an item in the cache based on its key, resetting its sliding expiration timeout (if any).
        /// </summary>
        /// <param name="key"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public void Remove(string key)
        {
            var request = new Request(CommandType.Del, key);
            _respClient.Execute(request);
            // TODO validate response
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (key.Length == 0)
                throw new ArgumentException(Resources.ArgumentCannotBeEmpty, nameof(key));

            var request = new Request(CommandType.Set, GetSetArgs(key, value, options));
            var response = _respClient.Execute(request); // TODO validate responses
        }

        /// <summary>
        /// Adds an item (as byte[]) to the cache using a string key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (key.Length == 0)
                throw new ArgumentException(Resources.ArgumentCannotBeEmpty, nameof(key));

            var request = new Request(CommandType.Set, GetSetArgs(key, value, options));
            var response = await _respClient.ExecuteAsync(request, token).ConfigureAwait(false);
            //if (responses.First().ValueType == Protocol.ValueType.)
            // TODO validate responses
        }

        private static object[] GetSetArgs(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            // TODO UTF8Parser
            if (options.SlidingExpiration.HasValue)
                return new object[] { key, value, $"PX {options.SlidingExpiration.Value.TotalMilliseconds:0}" };

            if (options.AbsoluteExpiration.HasValue)
                return new object[] { key, value, $"PX {(options.AbsoluteExpiration.Value - DateTimeOffset.Now).TotalMilliseconds:0}" };

            if (options.AbsoluteExpirationRelativeToNow.HasValue)
                return new object[] { key, value, $"PX {options.AbsoluteExpirationRelativeToNow.Value.TotalMilliseconds:0}" };

            return new object[] { key, value };
        }
    }
}