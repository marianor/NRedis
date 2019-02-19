namespace NRedis.Protocol
{
    public interface IResponse
    {
        object Value { get; }

        DataType DataType { get; }

        byte[] GetRawValue();
    }
}