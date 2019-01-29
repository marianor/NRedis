using Framework.Caching.Redis.Properties;
using Framework.Caching.Redis.Protocol;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Framework.Caching.Redis
{
    public class RedisCache : IDistributedCache
    {
        private static readonly byte[] m_valueField = Resp.Encoding.GetBytes("val");
        private static readonly byte[] m_slidingExpirationField = Resp.Encoding.GetBytes("sld");
        private static readonly byte[] m_refreshScript = Resp.Encoding.GetBytes("\"local sliding = tonumber(redis.call('HGET',KEYS[1],ARGV[1])) if sliding > 0 then redis.call('PEXPIRE',KEYS[1],sliding) end\"");

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

            var request = new Request(CommandType.HGet, key, m_valueField);
            var response = _respClient.Execute(request);
            ThrowIfError(response);
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

            var request = new Request(CommandType.HGet, key, m_valueField);
            var response = await _respClient.ExecuteAsync(request, token).ConfigureAwait(false);
            ThrowIfError(response);
            return response.GetRawValue();
        }

        public void Refresh(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (key.Length == 0)
                throw new ArgumentException(Resources.ArgumentCannotBeEmpty, nameof(key));

            var request = new Request(CommandType.Eval, m_refreshScript, 1, key, m_slidingExpirationField);
            var response = _respClient.Execute(request);
            ThrowIfError(response);
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

            var request = new Request(CommandType.Eval, m_refreshScript, 1, key, m_slidingExpirationField);
            var response = await _respClient.ExecuteAsync(request, token).ConfigureAwait(false);
            ThrowIfError(response);
        }

        public void Remove(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (key.Length == 0)
                throw new ArgumentException(Resources.ArgumentCannotBeEmpty, nameof(key));

            var request = new Request(CommandType.Del, key);
            var response = _respClient.Execute(request);
            ThrowIfError(response);
        }

        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (key.Length == 0)
                throw new ArgumentException(Resources.ArgumentCannotBeEmpty, nameof(key));

            var request = new Request(CommandType.Del, key);
            var response = await _respClient.ExecuteAsync(request, token).ConfigureAwait(false);
            ThrowIfError(response);
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

            var setRequest = new Request(CommandType.HMSet, key, m_valueField, value, m_slidingExpirationField, options.SlidingExpiration.GetValueOrDefault());
            var expirationRequest = GetExpirationRequest(key, options);
            if (expirationRequest == null)
            {
                var response = _respClient.Execute(setRequest);
                ThrowIfError(response);
            }
            else
            {
                var responses = _respClient.Execute(new[] { setRequest, expirationRequest });
                ThrowIfError(responses);
            }
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

            var setRequest = new Request(CommandType.HMSet, key, m_valueField, value, m_slidingExpirationField, options.SlidingExpiration.GetValueOrDefault());
            var expirationRequest = GetExpirationRequest(key, options);
            if (expirationRequest == null)
            {
                var response = await _respClient.ExecuteAsync(setRequest, token).ConfigureAwait(false);
                ThrowIfError(response);
            }
            else
            {
                var responses = await _respClient.ExecuteAsync(new[] { setRequest, expirationRequest }, token).ConfigureAwait(false);
                ThrowIfError(responses);
            }
        }

        private IRequest GetExpirationRequest(string key, DistributedCacheEntryOptions options)
        {
            if (options.AbsoluteExpirationRelativeToNow.HasValue)
                return new Request(CommandType.PExpire, key, options.AbsoluteExpirationRelativeToNow.Value);

            if (options.AbsoluteExpiration.HasValue)
                return new Request(CommandType.PExpireAt, key, options.AbsoluteExpiration.Value);

            if (options.SlidingExpiration.HasValue)
                return new Request(CommandType.PExpire, key, options.SlidingExpiration.Value);

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowIfError(IEnumerable<IResponse> responses)
        {
            foreach (var response in responses)
                ThrowIfError(response);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ThrowIfError(IResponse response)
        {
            if (response.DataType == DataType.Error)
                throw new ProtocolViolationException((string)response.Value);
        }
    }
}