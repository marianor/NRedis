using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Framework.Caching.Redis
{
    /// <summary>
    /// Configuration options for <see cref="RedisCache" />.
    /// </summary>
    public sealed class RedisCacheOptions : IOptions<RedisCacheOptions>
    {
        public string Host { get; set; }

        public ILoggerFactory LoggerFactory { get; set; }

        public int Port { get; set; } = 6380;

        public bool UseSsl { get; set; } = true;

        public string Password { get; set; }

        RedisCacheOptions IOptions<RedisCacheOptions>.Value => this;
    }
}