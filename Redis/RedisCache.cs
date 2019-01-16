using Framework.Caching.Redis.Protocol;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

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
            var request = new KeyRequest(CommandType.Get, key);
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
            var request = new KeyRequest(CommandType.Get, key);
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
            var request = new KeyRequest(CommandType.Del, key);
            _respClient.Execute(request);
            // TODO validate response
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            // TODO store bytes directy, remove the Encode if possible
            var requests = new List<IRequest> { new KeyValueRequest(CommandType.Set, key, value) };
            if (options.SlidingExpiration.HasValue)
                requests.Add(new PExpireRequest(key, options.SlidingExpiration.Value));
            else if (options.AbsoluteExpiration.HasValue)
                requests.Add(new PExpireAtRequest(key, options.AbsoluteExpiration.Value));
            else if (options.AbsoluteExpirationRelativeToNow.HasValue)
                requests.Add(new PExpireAtRequest(key, DateTimeOffset.Now + options.AbsoluteExpirationRelativeToNow.Value));

            var responses = _respClient.Execute(requests); // TODO validate responses
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
            var requests = new List<IRequest> { new KeyValueRequest(CommandType.Set, key, value) };
            if (options.SlidingExpiration.HasValue)
                requests.Add(new PExpireRequest(key, options.SlidingExpiration.Value));
            else if (options.AbsoluteExpiration.HasValue)
                requests.Add(new PExpireAtRequest(key, options.AbsoluteExpiration.Value));
            else if (options.AbsoluteExpirationRelativeToNow.HasValue)
                requests.Add(new PExpireAtRequest(key, DateTimeOffset.Now + options.AbsoluteExpirationRelativeToNow.Value));

            var responses = await _respClient.ExecuteAsync(requests, token).ConfigureAwait(false);
            //if (responses.First().ValueType == Protocol.ValueType.)
            // TODO validate responses
        }
    }
}