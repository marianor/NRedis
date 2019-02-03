namespace Framework.Caching.Redis.Protocol
{
    public interface IResponse
    {
        object Value { get; }

        DataType DataType { get; }

        byte[] GetRawValue();
    }
}