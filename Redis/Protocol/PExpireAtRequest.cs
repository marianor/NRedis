using System;
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

        public override Memory<byte> Buffer => Encoding.UTF8.GetBytes(Command + " " + Key + " " + AbsoluteExpiration.ToUnixTimeMilliseconds() + (char)RespClient.CR + (char)RespClient.LF);
    }
}