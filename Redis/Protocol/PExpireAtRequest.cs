using System;
using System.Globalization;
using System.Text;

namespace Framework.Caching.Protocol
{
    public class PExpireAtRequest : KeyRequest
    {
        public PExpireAtRequest(string key, DateTimeOffset absoluteExpiration) : base(CommandType.PExpireAt.ToCommand(), key)
        {
            AbsoluteExpiration = absoluteExpiration;
        }

        private DateTimeOffset AbsoluteExpiration { get; }

        public override int Write(Memory<byte> buffer)
        {
            var writer = new MemoryWriter(buffer);
            writer.Write(Command);
            writer.Write(RespProtocol.Separator);
            writer.Write(Key);
            writer.Write(RespProtocol.Separator);
            writer.Write(AbsoluteExpiration.ToUnixTimeMilliseconds());
            writer.Write(RespProtocol.CRLF);
            return writer.Position;
        }
    }
}