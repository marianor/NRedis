using Framework.Caching.Protocol;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Caching
{
    public class RedisCache : IDistributedCache
    {
        private readonly IRespClient _respClient;

        public RedisCache(IRespClient client)
        {
            _respClient = client ?? throw new ArgumentNullException(nameof(client));
        }

        public byte[] Get(string key)
        {
            var response = _respClient.Execute(new KeyRequest(CommandType.Get, key))[0];
            if (response.Value == null)
                return null;
            return Convert.FromBase64String(response.Value as string);
        }

        /// <summary>
        /// Takes a string key and retrieves a cached item as a byte[] if found in the cache.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<byte[]> GetAsync(string key, CancellationToken token = default(CancellationToken))
        {
            var requests = new IRequest[] { new KeyRequest(CommandType.Get, key) };
            var responses = await _respClient.ExecuteAsync(requests, token).ConfigureAwait(false);
            var response = responses[0];
            if (response.Value == null)
                return null;
            return Convert.FromBase64String(response.Value as string);
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
        public Task RefreshAsync(string key, CancellationToken token = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public void Remove(string key)
        {
            _respClient.Execute(new KeyRequest(CommandType.Del, key));
        }

        public Task RemoveAsync(string key, CancellationToken token = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            var requests = new List<IRequest> { new KeyValueRequest(CommandType.Set, key, Convert.ToBase64String(value)) };
            if (options.SlidingExpiration.HasValue)
                requests.Add(new PExpireRequest(key, options.SlidingExpiration.Value));
            else if (options.AbsoluteExpiration.HasValue)
                requests.Add(new PExpireAtRequest(key, options.AbsoluteExpiration.Value));
            else if (options.AbsoluteExpirationRelativeToNow.HasValue)
                requests.Add(new PExpireAtRequest(key, DateTimeOffset.Now + options.AbsoluteExpirationRelativeToNow.Value));

            var responses = _respClient.Execute(requests.ToArray()); // TODO validate responses
        }

        /// <summary>
        /// Adds an item (as byte[]) to the cache using a string key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default(CancellationToken))
        {
            var requests = new List<IRequest> { new KeyValueRequest(CommandType.Set, key, Convert.ToBase64String(value)) };
            if (options.SlidingExpiration.HasValue)
                requests.Add(new PExpireRequest(key, options.SlidingExpiration.Value));
            else if (options.AbsoluteExpiration.HasValue)
                requests.Add(new PExpireAtRequest(key, options.AbsoluteExpiration.Value));
            else if (options.AbsoluteExpirationRelativeToNow.HasValue)
                requests.Add(new PExpireAtRequest(key, DateTimeOffset.Now + options.AbsoluteExpirationRelativeToNow.Value));

            var responses = await _respClient.ExecuteAsync(requests.ToArray(), token).ConfigureAwait(false); // TODO validate responses
        }
    }
}