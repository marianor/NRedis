using Framework.Caching.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Framework.Caching.Protocol
{
    public class KeysRequest : Request
    {
        public KeysRequest(string command, IEnumerable<string> keys) : base(command)
        {
            Keys = keys ?? throw new ArgumentNullException(nameof(keys));
            if (!keys.Any())
                throw new ArgumentException(Resources.ArgumentCannotBeEmpty, nameof(keys));
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

        public IEnumerable<string> Keys { get; }

        public override int Write(Memory<byte> buffer)
        {
            var index = 0;
            var span = buffer.Span;
            foreach (var b in Encoding.UTF8.GetBytes(Command + " " + string.Join(" ", Keys)))
                span[index++] = b;

            span[index++] = RespClient.CR;
            span[index++] = RespClient.LF;
            return index;
        }
    }
}