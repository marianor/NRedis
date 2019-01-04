using Framework.Caching.Properties;
using System;
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
            var index = 0;
            var span = buffer.Span;
            foreach (var b in Encoding.UTF8.GetBytes(Command + " " + Key + " " + SlidingExpiration.TotalMilliseconds))
                span[index++] = b;

            span[index++] = RespProtocol.CR;
            span[index++] = RespProtocol.LF;
            return index;
        }
    }
}