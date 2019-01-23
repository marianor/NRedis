using System.Buffers.Text;

namespace Framework.Caching.Redis.Protocol
{
    public class IntegerResponse : IResponse
    {
        private readonly byte[] _value;

        internal IntegerResponse(byte[] value) => _value = value;

        public long Value => Utf8Parser.TryParse(_value, out long value, out int bytesConsumed) ? value : default;

        object IResponse.Value => Value;

        public DataType DataType => DataType.Integer;

        public byte[] GetRawValue()
        {
            return _value;
        }
    }
}