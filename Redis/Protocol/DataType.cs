namespace Framework.Caching.Redis.Protocol
{
    public enum DataType
    {
        Array,
        BulkString,
        Error,
#pragma warning disable CA1720 // Identifier contains type name
        Integer,
#pragma warning restore CA1720 // Identifier contains type name
        SimpleString
    }
}