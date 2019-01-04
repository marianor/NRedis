using Framework.Caching.Properties;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Globalization;
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
                    throw new ProtocolViolationException(string.Format(CultureInfo.CurrentCulture, Resources.ProtocolViolationInvalidBeginChar, _index));
            }
        }

        private string ParseSimpleString()
        {
            var buffer = _response.AsSpan(_index);
            var length = buffer.IndexOf(RespProtocol.CRLF);
            if (length == -1)
                throw new ProtocolViolationException(string.Format(CultureInfo.CurrentCulture, Resources.ProtocolViolationInvalidEndChar, _index));

            _index += length + 2;
            return Encoding.UTF8.GetString(buffer.Slice(0, length).ToArray());
        }

        private long ParseInteger()
        {
            var buffer = _response.AsSpan(_index);
            var length = buffer.IndexOf(RespProtocol.CRLF);
            if (length == -1)
                throw new ProtocolViolationException(string.Format(CultureInfo.CurrentCulture, Resources.ProtocolViolationInvalidEndChar, _index));

            if (!Utf8Parser.TryParse(buffer, out long value, out int bytesConsumed) && bytesConsumed != length)
                throw new ProtocolViolationException(string.Format(CultureInfo.CurrentCulture, Resources.ProtocolViolationParsingInteger, _index));

            _index += length + 2;
            return value;
        }

        private string ParseBulkString()
        {
            var length = (int)ParseInteger();
            if (length == -1)
                return null;

            var value = Encoding.UTF8.GetString(_response, _index, length);
            _index += length;

            // TODO compare Spans ???
            if (_index >= _response.Length || _response[_index++] != RespProtocol.CRLF[0] || _response[_index++] != RespProtocol.CRLF[1])
                throw new ProtocolViolationException(string.Format(CultureInfo.CurrentCulture, Resources.ProtocolViolationInvalidEndChar, _index));

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