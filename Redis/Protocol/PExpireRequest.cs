using System;

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

        private protected override void WritePayload(MemoryWriter writer)
        {
            base.WritePayload(writer);
            writer.Write(Protocol.Separator);
            writer.Write(SlidingExpiration.TotalMilliseconds);
        }
    }
}