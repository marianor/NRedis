using System;
using System.Collections.Generic;

namespace NRedis.Protocol
{
    public class ArrayResponse : IResponse
    {
        internal ArrayResponse(in object[] value) => Value = value;

        public IEnumerable<object> Value { get; }

        object IResponse.Value => Value;

        public DataType DataType => DataType.Array;

        public byte[] GetRawValue() => throw new NotImplementedException();
    }
}