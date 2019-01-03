using System;

namespace Framework.Caching.Protocol
{
    public interface IRequest
    {
        string Command { get; }

        int Write(Memory<byte> buffer);
    }
}