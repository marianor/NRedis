using Framework.Caching.Redis.Properties;
using Framework.Caching.Redis.Protocol;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Caching.Redis
{
    public class RedisCache : IDistributedCache
    {
        private static readonly byte[] ValueField = RespProtocol.Encoding.GetBytes("val");
        private static readonly byte[] SlidingExpirationField = RespProtocol.Encoding.GetBytes("sld");
        private static readonly byte[] RefreshScript = RespProtocol.Encoding.GetBytes("\"local sliding = tonumber(redis.call('HGET',KEYS[1],ARGV[1])) if sliding > 0 then redis.call('PEXPIRE',KEYS[1],sliding) end\"");

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

            var request = new Request(CommandType.HGet, key, ValueField);
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

            var request = new Request(CommandType.HGet, key, ValueField);
            var response = await _respClient.ExecuteAsync(request, token).ConfigureAwait(false);
            return response.GetRawValue();
        }

        public void Refresh(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (key.Length == 0)
                throw new ArgumentException(Resources.ArgumentCannotBeEmpty, nameof(key));

            throw new NotImplementedException();
        }

        /// <summary>
        /// Refreshes an item in the cache based on its key, resetting its sliding expiration timeout (if any).
        /// </summary>
        /// <param name="key"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task RefreshAsync(string key, CancellationToken token = default)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (key.Length == 0)
                throw new ArgumentException(Resources.ArgumentCannotBeEmpty, nameof(key));

            var request = new Request(CommandType.Eval, RefreshScript, 1, key, SlidingExpirationField);
            var response = await _respClient.ExecuteAsync(request, token).ConfigureAwait(false);
            if (response.DataType == DataType.Error)
                throw new ProtocolViolationException((string)response.Value);
        }

        public void Remove(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (key.Length == 0)
                throw new ArgumentException(Resources.ArgumentCannotBeEmpty, nameof(key));

            var request = new Request(CommandType.Del, key);
            _respClient.Execute(request);
            // TODO validate response
        }

        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (key.Length == 0)
                throw new ArgumentException(Resources.ArgumentCannotBeEmpty, nameof(key));

            var request = new Request(CommandType.Del, key);
            await _respClient.ExecuteAsync(request, token).ConfigureAwait(false);
            // TODO validate response
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (key.Length == 0)
                throw new ArgumentException(Resources.ArgumentCannotBeEmpty, nameof(key));
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var now = DateTimeOffset.UtcNow;
            var slidingExpiration = options.SlidingExpiration.GetValueOrDefault();

            var request = new Request(CommandType.HMSet, key, ValueField, value, SlidingExpirationField, slidingExpiration);
            if (options.AbsoluteExpirationRelativeToNow.HasValue)
                _respClient.Execute(new[] { request, new Request(CommandType.PExpire, key, options.AbsoluteExpirationRelativeToNow.Value) });
            else if (options.AbsoluteExpiration.HasValue)
                _respClient.Execute(new[] { request, new Request(CommandType.PExpireAt, key, options.AbsoluteExpiration.Value) });
            else if (options.SlidingExpiration.HasValue)
                _respClient.Execute(new[] { request, new Request(CommandType.PExpire, key, options.SlidingExpiration.Value) });
            else
                _respClient.Execute(request);
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
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var now = DateTimeOffset.UtcNow;
            var slidingExpiration = options.SlidingExpiration.GetValueOrDefault();

            var request = new Request(CommandType.HMSet, key, ValueField, value, SlidingExpirationField, slidingExpiration);
            if (options.AbsoluteExpirationRelativeToNow.HasValue)
                await _respClient.ExecuteAsync(new[] { request, new Request(CommandType.PExpire, key, options.AbsoluteExpirationRelativeToNow.Value) }, token).ConfigureAwait(false);
            else if (options.AbsoluteExpiration.HasValue)
                await _respClient.ExecuteAsync(new[] { request, new Request(CommandType.PExpireAt, key, options.AbsoluteExpiration.Value) }, token).ConfigureAwait(false);
            else if (options.SlidingExpiration.HasValue)
                await _respClient.ExecuteAsync(new[] { request, new Request(CommandType.PExpire, key, options.SlidingExpiration.Value) }, token).ConfigureAwait(false);
            else
                await _respClient.ExecuteAsync(request, token).ConfigureAwait(false);
            //if (responses.First().ValueType == Protocol.ValueType.)
            // TODO validate responses
        }
    }
}