using System;

namespace Framework.Caching.Redis.Protocol
{
    public interface IRequest
    {
        string Command { get; }

        object[] GetArgs();

        T GetArg<T>(int index);
    }
}