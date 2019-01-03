using Framework.Caching.Properties;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Framework.Caching.Protocol
{
    // TODO refactor tu use new System.Buffers.Text.Utf8Parser and System.Buffers.Text.Utf8Formatter??????
    internal class RespParser
    {
        private const byte SimpleString = (byte)'+';
        private const byte Error = (byte)'-';
        private const byte Integer = (byte)':';
        private const byte BulkString = (byte)'$';
        private const byte Array = (byte)'*';

        private byte[] _response;
        private int _index;

        public RespParser(byte[] response)
        {
            _response = response;
        }

        public IEnumerable<IResponse> Parse()
        {
            var responses = new List<IResponse>();
            while (_response.Length > _index)
                yield return ParseElement();
        }

        private IResponse ParseElement()
        {
            switch (_response[_index++])
            {
                case SimpleString:
                    return new StringResponse(ValueType.SimpleString, ParseSimpleString());
                case Error:
                    return new StringResponse(ValueType.Error, ParseSimpleString());
                case Integer:
                    return new IntegerResponse(ParseInteger());
                case BulkString:
                    return new StringResponse(ValueType.BulkString, ParseBulkString());
                case Array:
                    return new ArrayResponse(ParseArray());
                default:
                    throw new ProtocolViolationException(Resources.ProtocolViolationInvalidBeginChar);
            }
        }

        private string ParseSimpleString()
        {
            var END1 = new ReadOnlySpan<byte>(new[] { RespClient.CR, RespClient.LF });
            var value = _response.AsSpan(_index);
            var length = value.IndexOf(END1);

            if (length == -1)
                throw new ProtocolViolationException(Resources.ProtocolViolationInvalidEndChar);

            _index += length + 2;
            return Encoding.UTF8.GetString(value.Slice(0, length).ToArray());
        }

        private int ParseInteger()
        {
            var number = 0;
            var sign = 1;

            var c = _response[_index];
            if (c == '-')
            {
                sign = -1;
                _index++;
            }

            c = _response[_index++];
            while (_index < _response.Length && c != RespClient.CR)
            {
                number = number * 10 + c - '0';
                c = _response[_index++];
            }

            if (_index >= _response.Length || _response[_index++] != RespClient.LF)
                throw new ProtocolViolationException(Resources.ProtocolViolationInvalidEndChar);

            return number * sign;
        }

        private string ParseBulkString()
        {
            var length = ParseInteger();
            if (length == -1)
                return null;

            var value = Encoding.UTF8.GetString(_response, _index, length);
            _index += length;

            // TODO compare Spans
            if (_index >= _response.Length || _response[_index++] != RespClient.CR || _response[_index++] != RespClient.LF)
                throw new ProtocolViolationException(Resources.ProtocolViolationInvalidEndChar);

            return value;
        }

        private object[] ParseArray()
        {
            var length = ParseInteger();
            if (length == -1)
                return null;

            var array = new object[length];
            for (var arrayIndex = 0; arrayIndex < length; arrayIndex++)
                array[arrayIndex] = ParseElement().Value;

            return array;
        }
    }
}