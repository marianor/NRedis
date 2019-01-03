using System;

namespace Framework.Caching.Protocol
{
    public interface IRequest
    {
        Memory<byte> Buffer { get; }

        string Command { get; }
    }
}