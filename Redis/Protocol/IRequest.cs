using System;

namespace Framework.Caching.Redis.Protocol
{
    public interface IRequest
    {
        string Command { get; }

        int Length { get; }

        object[] GetArgs();

        T GetArg<T>(int index);

        // TODO remove from here, put this logic in RespFormatter
        int Write(Memory<byte> buffer);
    }
}