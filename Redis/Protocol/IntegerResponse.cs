using System;
using System.Buffers.Text;

namespace NRedis.Protocol
{
    public class IntegerResponse : IResponse
    {
        private readonly byte[] _rawValue;
        private readonly Lazy<long> _value;

        internal IntegerResponse(in byte[] rawValue)
        {
            _rawValue = rawValue;
            _value = new Lazy<long>(() => Utf8Parser.TryParse(_rawValue, out long value, out _) ? value : default);
        }

        public long Value => _value.Value;

        object IResponse.Value => _value.Value;

        public DataType DataType => DataType.Integer;

        public byte[] GetRawValue() => _rawValue;
    }
}