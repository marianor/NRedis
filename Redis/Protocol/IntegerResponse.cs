namespace Framework.Caching.Protocol
{
    public class IntegerResponse : IResponse
    {
        internal IntegerResponse(int value)
        {
            Value = value;
        }

        public int Value { get; }

        object IResponse.Value => Value;

        public ValueType ValueType => ValueType.Integer;
    }
}