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

        public override Memory<byte> Buffer => new Memory<byte>(Encoding.UTF8.GetBytes(Command + " " + Key + " " + SlidingExpiration.TotalMilliseconds + (char)RespClient.CR + (char)RespClient.LF));
    }
}