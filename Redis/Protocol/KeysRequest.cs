using Framework.Caching.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Framework.Caching.Protocol
{
    public class KeysRequest : IRequest
    {
        public KeysRequest(RequestType requestType, params string[] keys) : this(requestType, (IEnumerable<string>)keys)
        {
        }

        public KeysRequest(RequestType requestType, IEnumerable<string> keys)
        {
            Keys = keys ?? throw new ArgumentNullException(nameof(keys));
            if (!keys.Any())
                throw new ArgumentException(Resources.ArgumentCannotBeEmpty, nameof(keys));

            RequestType = requestType;
            Keys = keys;
        }

        public IEnumerable<string> Keys { get; }

        public RequestType RequestType { get; }

        public string RequestText => RequestType.ToString().ToUpperInvariant() + " " + string.Join(" ", Keys) + RespClient.CR + RespClient.LF;
    }
}