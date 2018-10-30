namespace Framework.Caching.Protocol
{
    public interface IResponse
    {
        object Value { get; }

        ValueType ValueType { get; }
    }
}