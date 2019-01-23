namespace Framework.Caching.Redis.Protocol
{
    public class StringResponse : IResponse
    {
        private readonly byte[] _value;

        internal StringResponse(DataType valueType, byte[] value)
        {
            DataType = valueType;
            _value = value;
        }

        public DataType DataType { get; }

        public string Value => _value == null ? null : RespProtocol.Encoding.GetString(_value);

        object IResponse.Value => Value;

        public byte[] GetRawValue()
        {
            return _value;
        }
    }
}