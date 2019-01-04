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
            var writer = new MemoryWriter(buffer);
            writer.Write(Command);
            writer.Write(RespProtocol.Separator);
            writer.Write(Key);
            writer.Write(RespProtocol.CRLF);
            return writer.Position;
        }
    }
}