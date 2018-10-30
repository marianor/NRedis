using System.Collections.Generic;

namespace Framework.Caching.Protocol
{
    public class ArrayResponse : IResponse
    {
        internal ArrayResponse(object[] value)
        {
            Value = value;
        }

        public IEnumerable<object> Value { get; }

        object IResponse.Value => Value;

        public ValueType ValueType => ValueType.Array;
    }
}