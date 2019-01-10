using Microsoft.Extensions.Options;

namespace Framework.Caching
{
    /// <summary>
    /// Configuration options for <see cref="Framework.Caching.Redis.RedisCache" />.
    /// </summary>
    public sealed class RedisCacheOptions : IOptions<RedisCacheOptions>
    {
        public string Host { get; set; }

        public int Port { get; set; } = 6380;

        public bool UseSsl { get; set; } = true;

        // TODO convert to SecureString
        public string Password { get; set; }

        RedisCacheOptions IOptions<RedisCacheOptions>.Value => this;
    }
}