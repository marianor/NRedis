using Framework.Caching.Properties;
using System;

namespace Framework.Caching.Protocol
{
    public class KeyValueRequest : IRequest
    {
        public KeyValueRequest(RequestType requestType, string key, string value)
        {
            RequestType = requestType;
            Key = key ?? throw new ArgumentNullException(nameof(key));
            if (key.Length == 0)
                throw new ArgumentException(Resources.ArgumentCannotBeEmpty, nameof(key));

            Value = value;
        }

        private string Key { get; }

        private string Value { get; }

        public RequestType RequestType { get; }

        public string RequestText => RequestType.ToString().ToUpperInvariant() + " " + Key + " " + Value + RespClient.CR + RespClient.LF;
    }
}