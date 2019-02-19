namespace NRedis.Protocol
{
    public interface IRequest
    {
        string Command { get; }

        object[] GetArgs();

        T GetArg<T>(int index);
    }
}