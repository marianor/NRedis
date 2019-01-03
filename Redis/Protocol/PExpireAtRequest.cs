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

        public override int Write(Memory<byte> buffer)
        {
            var index = 0;
            var span = buffer.Span;
            foreach (var b in Encoding.UTF8.GetBytes(Command + " " + Key + " " + AbsoluteExpiration.ToUnixTimeMilliseconds()))
                span[index++] = b;

            span[index++] = RespClient.CR;
            span[index++] = RespClient.LF;
            return index;
        }
    }
}