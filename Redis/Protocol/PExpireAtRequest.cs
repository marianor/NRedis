using System;

namespace Framework.Caching.Redis.Protocol
{
    public class PExpireAtRequest : KeyRequest
    {
        public PExpireAtRequest(string key, DateTimeOffset absoluteExpiration) : base(CommandType.PExpireAt.ToCommand(), key)
        {
            AbsoluteExpiration = absoluteExpiration;
        }

        public DateTimeOffset AbsoluteExpiration { get; }

        private protected override void WritePayload(MemoryWriter writer)
        {
            base.WritePayload(writer);
            writer.Write(RespProtocol.Separator);
            writer.Write(AbsoluteExpiration.ToUnixTimeMilliseconds());
        }
    }
}