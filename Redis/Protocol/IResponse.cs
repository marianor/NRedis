namespace Framework.Caching.Redis.Protocol
{
    public interface IResponse
    {
        object Value { get; }

        ValueType ValueType { get; }
    }
}