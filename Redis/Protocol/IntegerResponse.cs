﻿using System.Buffers.Text;

namespace Framework.Caching.Redis.Protocol
{
    public class IntegerResponse : IResponse
    {
        // TODO change by ReadOnlyMemory<T>
        private readonly byte[] _value;

        internal IntegerResponse(in byte[] value) => _value = value;

        public long Value => Utf8Parser.TryParse(_value, out long value, out _) ? value : default;

        object IResponse.Value => Value;

        public DataType DataType => DataType.Integer;

        public byte[] GetRawValue() => _value;
    }
}