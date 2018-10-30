using Framework.Caching.Properties;
using System;

namespace Framework.Caching.Protocol
{
    public class PExpireAtRequest : IRequest
    {
        public PExpireAtRequest(string key, DateTimeOffset absoluteExpiration)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            if (key.Length == 0)
                throw new ArgumentException(Resources.ArgumentCannotBeEmpty, nameof(key));

            AbsoluteExpiration = absoluteExpiration;
        }

        private DateTimeOffset AbsoluteExpiration { get; }

        private string Key { get; }

        public RequestType RequestType => RequestType.PExpireAt;

        public string RequestText => RequestType.ToString().ToUpperInvariant() + " " + Key + " " + AbsoluteExpiration.ToUnixTimeMilliseconds() + RespClient.CR + RespClient.LF;
    }
}