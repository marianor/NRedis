using Framework.Caching.Properties;
using System;

namespace Framework.Caching.Protocol
{
    public class KeyRequest : IRequest
    {
        public KeyRequest(RequestType requestType, string key)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            if (key.Length == 0)
                throw new ArgumentException(Resources.ArgumentCannotBeEmpty, nameof(key));

            RequestType = requestType;
            Key = key;
        }

        public string Key { get; }

        public RequestType RequestType { get; }

        public string RequestText => RequestType.ToString().ToUpperInvariant() + " " + Key + RespClient.CR + RespClient.LF;
    }
}