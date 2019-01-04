namespace Framework.Caching.Protocol
{
    public class IntegerResponse : IResponse
    {
        internal IntegerResponse(long value) => Value = value;

        public long Value { get; }

        object IResponse.Value => Value;

        public ValueType ValueType => ValueType.Integer;
    }
}