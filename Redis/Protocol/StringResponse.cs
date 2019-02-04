using System;

namespace Framework.Caching.Redis.Protocol
{
    public class StringResponse : IResponse
    {
        // TODO change by ReadOnlyMemory<T>
        private readonly byte[] _value;

        internal StringResponse(in DataType valueType, in byte[] value)
        {
            DataType = valueType;
            _value = value;
        }

        public DataType DataType { get; }

        public string Value => _value == null ? null : Resp.Encoding.GetString(_value);

        object IResponse.Value => Value;

        public byte[] GetRawValue() => _value;
    }
}