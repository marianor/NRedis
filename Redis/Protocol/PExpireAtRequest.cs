﻿using System;

namespace Framework.Caching.Protocol
{
    public class PExpireAtRequest : KeyRequest
    {
        public PExpireAtRequest(string key, DateTimeOffset absoluteExpiration) : base(CommandType.PExpireAt.ToCommand(), key)
        {
            AbsoluteExpiration = absoluteExpiration;
        }

        private DateTimeOffset AbsoluteExpiration { get; }

        private protected override void WritePayload(MemoryWriter writer)
        {
            base.WritePayload(writer);
            writer.Write(Protocol.Separator);
            writer.Write(AbsoluteExpiration.ToUnixTimeMilliseconds());
        }
    }
}