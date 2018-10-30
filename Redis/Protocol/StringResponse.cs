namespace Framework.Caching.Protocol
{
    public class StringResponse : IResponse
    {
        internal StringResponse(ValueType valueType, string value)
        {
            ValueType = valueType;
            Value = value;
        }

        public ValueType ValueType { get; }

        public string Value { get; }

        object IResponse.Value => Value;
    }
}