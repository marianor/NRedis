using Framework.Caching.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Framework.Caching.Protocol
{
    public class KeysRequest : Request
    {
        private readonly byte[][] _keys;

        public KeysRequest(string command, IEnumerable<string> keys) : base(command)
        {
            if (keys == null)
                throw new ArgumentNullException(nameof(keys));
            if (!keys.Any())
                throw new ArgumentException(Resources.ArgumentCannotBeEmpty, nameof(keys));

            _keys = keys.Select(k => Protocol.Encoding.GetBytes(k)).ToArray();
        }

        public KeysRequest(string command, params string[] keys) : this(command, (IEnumerable<string>)keys)
        {
        }

        public KeysRequest(CommandType commandType, IEnumerable<string> keys) : this(commandType.ToCommand(), keys)
        {
        }

        public KeysRequest(CommandType commandType, params string[] keys) : this(commandType.ToCommand(), keys)
        {
        }

        public IEnumerable<string> Keys => _keys.Select(k => Protocol.Encoding.GetString(k));

        private protected override void WritePayload(MemoryWriter writer)
        {
            foreach (var key in _keys)
            {
                writer.Write(Protocol.Separator);
                writer.Write(key);
            }
        }
    }
}