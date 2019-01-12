using System;

namespace Framework.Caching.Redis.Protocol
{
    public interface IRequest
    {
        string Command { get; }

        int Write(Memory<byte> buffer);
    }
}