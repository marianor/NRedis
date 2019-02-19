using NRedis;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace NRedis.DependencyInjection
{
    public static class RedisCacheServiceCollectionExtensions
    {
        public static IServiceCollection AddDistributedRedisCache(this IServiceCollection services, Action<RedisCacheOptions> setupAction)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (setupAction == null)
                throw new ArgumentNullException(nameof(setupAction));

            services.Configure(setupAction);
            services.TryAdd(ServiceDescriptor.Singleton<IDistributedCache, RedisCache>());
            return services;
        }
    }
}