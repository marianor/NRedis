using Framework.Caching.Properties;
using System;

namespace Framework.Caching.Protocol
{
    public class PExpireRequest : IRequest
    {
        public PExpireRequest(string key, TimeSpan slidingExpiration)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            if (key.Length == 0)
                throw new ArgumentException(Resources.ArgumentCannotBeEmpty, nameof(key));
            if (slidingExpiration <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(slidingExpiration));

            SlidingExpiration = slidingExpiration;
        }

        public string Key { get; }

        public TimeSpan SlidingExpiration { get; }

        public RequestType RequestType => RequestType.PExpire;

        public string RequestText => RequestType.ToString().ToUpperInvariant() + " " + Key + " " + SlidingExpiration.TotalMilliseconds + RespClient.CR + RespClient.LF;
    }
}