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

        public override Memory<byte> Buffer => Encoding.UTF8.GetBytes(Command + " " + string.Join(" ", Keys) + (char)RespClient.CR + (char)RespClient.LF);
    }
}