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
            var writer = new MemoryWriter(buffer);
            writer.Write(Command);
            writer.Write(RespProtocol.Separator);
            writer.Write(Key);
            writer.Write(RespProtocol.Separator);
            writer.Write(Value);
            writer.Write(RespProtocol.CRLF);
            return writer.Position;
        }
    }
}