using System;
using System.Collections;
using System.Collections.Generic;

namespace Framework.Caching.Redis.Protocol
{
    public interface IRequest
    {
        string Command { get; }

        object[] GetArgs();

        T GetArg<T>(int index);

        int Write(Memory<byte> buffer);
    }
}