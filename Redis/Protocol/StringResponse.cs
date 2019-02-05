using System;

namespace Framework.Caching.Redis.Protocol
{
    public class StringResponse : IResponse
    {
        private readonly byte[] _rawValue;
        private readonly Lazy<string> _value;

        internal StringResponse(in DataType valueType, in byte[] rawValue)
        {
            DataType = valueType;
            _rawValue = rawValue;
            _value = new Lazy<string>(() => _rawValue == null ? null : Resp.Encoding.GetString(_rawValue));
        }

        public DataType DataType { get; }

        public string Value => _value.Value;

        object IResponse.Value => _value.Value;

        public byte[] GetRawValue() => _rawValue;
    }
}