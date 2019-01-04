using Framework.Caching.Properties;
using System;
using System.Text;

namespace Framework.Caching.Protocol
{
    public class KeyRequest : Request
    {
        public KeyRequest(string command, string key) : base(command)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            if (key.Length == 0)
                throw new ArgumentException(Resources.ArgumentCannotBeEmpty, nameof(key));
        }

        public KeyRequest(CommandType commandType, string key) : this(commandType.ToCommand(), key)
        {
        }

        public string Key { get; }

        public override int Write(Memory<byte> buffer)
        {
            var index = 0;
            var span = buffer.Span;
            foreach (var b in Encoding.UTF8.GetBytes(Command + " " + Key))
                span[index++] = b;

            span[index++] = RespProtocol.CR;
            span[index++] = RespProtocol.LF;
            return index;
        }
    }
}