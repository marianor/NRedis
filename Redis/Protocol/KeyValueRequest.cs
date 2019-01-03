using System;
using System.Text;

namespace Framework.Caching.Protocol
{
    public class KeyValueRequest : KeyRequest
    {
        public KeyValueRequest(string command, string key, string value) : base(command, key)
        {
            Value = value;
        }

        public KeyValueRequest(CommandType commandType, string key, string value) : this(commandType.ToCommand(), key, value)
        {
        }

        public string Value { get; }

        public override int Write(Memory<byte> buffer)
        {
            var index = 0;
            var span = buffer.Span;
            foreach (var b in Encoding.UTF8.GetBytes(Command + " " + Key + " " + Value))
                span[index++] = b;

            span[index++] = RespClient.CR;
            span[index++] = RespClient.LF;
            return index;
        }
    }
}