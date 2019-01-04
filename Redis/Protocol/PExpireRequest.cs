using System;
using System.Globalization;
using System.Text;

namespace Framework.Caching.Protocol
{
    public class PExpireRequest : KeyRequest
    {
        public PExpireRequest(string key, TimeSpan slidingExpiration) : base(CommandType.PExpire.ToCommand(), key)
        {
            if (slidingExpiration <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(slidingExpiration));

            SlidingExpiration = slidingExpiration;
        }

        public TimeSpan SlidingExpiration { get; }

        public override int Write(Memory<byte> buffer)
        {
            var writer = new MemoryWriter(buffer);
            writer.Write(Command);
            writer.Write(RespProtocol.Separator);
            writer.Write(Key);
            writer.Write(RespProtocol.Separator);
            writer.Write(SlidingExpiration.TotalMilliseconds);
            writer.Write(RespProtocol.CRLF);
            return writer.Position;
        }
    }
}