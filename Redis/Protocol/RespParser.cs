using Framework.Caching.Redis.Properties;
using System;
using System.Buffers;
using System.Buffers.Text;
using System.Net;

namespace Framework.Caching.Redis.Protocol
{
    internal static class RespParser
    {
        public static IResponse Parse(this in ReadOnlySequence<byte> buffer)
        {
            var position = 0;
            var response = ParseElement(buffer, ref position);
            if (position < buffer.Length)
                throw new ProtocolViolationException(Resources.ProtocolViolationNotExpectedData.Format(position));

            return response;
        }

        // TODO Consider Memory<byte>
        public static IResponse[] Parse(this in ReadOnlySequence<byte> buffer, int count)
        {
            var i = 0;
            var responses = new IResponse[count];

            var position = 0;
            while (buffer.Length > position)
                responses[i++] = buffer.ParseElement(ref position);

            if (i < count || i > count)
                throw new ProtocolViolationException("Invalid responses"); // TODO check messages

            return responses;
        }

        private static IResponse ParseElement(this in ReadOnlySequence<byte> buffer, ref int position)
        {
            var span = buffer.Slice(position++, 1).AsSpan();
            switch (span[0])
            {
                case Resp.BulkString:
                    return new StringResponse(DataType.BulkString, buffer.ParseBulkString(ref position));
                case Resp.Integer:
                    return new IntegerResponse(buffer.ParseInteger(ref position));
                case Resp.Error:
                    return new StringResponse(DataType.Error, buffer.ParseSimpleString(ref position));
                case Resp.SimpleString:
                    return new StringResponse(DataType.SimpleString, buffer.ParseSimpleString(ref position));
                case Resp.Array:
                    return new ArrayResponse(buffer.ParseArray(ref position));
                default:
                    throw new ProtocolViolationException(Resources.ProtocolViolationInvalidBeginChar.Format(position - 1));
            }
        }

        private static byte[] ParseSimpleString(this in ReadOnlySequence<byte> buffer, ref int position)
        {
            var span = buffer.Slice(position).AsSpan();
            var length = span.IndexOf(Resp.CRLF);
            if (length == -1)
                throw new ProtocolViolationException(Resources.ProtocolViolationInvalidEndChar.Format(position));

            position += length + 2;
            return span.Slice(0, length).ToArray(); // TODO MemoryMarshal
        }

        private static byte[] ParseInteger(this in ReadOnlySequence<byte> buffer, ref int position)
        {
            var span = buffer.Slice(position).AsSpan();
            var length = span.IndexOf(Resp.CRLF);
            if (length == -1)
                throw new ProtocolViolationException(Resources.ProtocolViolationInvalidEndChar.Format(position));

            position += length + 2;
            return span.Slice(0, length).ToArray(); // TODO avoid reallocation
        }

        private static byte[] ParseBulkString(this in ReadOnlySequence<byte> buffer, ref int position)
        {
            var lengthBuffer = ParseInteger(buffer, ref position);
            if (!Utf8Parser.TryParse(lengthBuffer, out int length, out _))
                throw new ProtocolViolationException(Resources.ProtocolViolationParsingInteger.Format(position));

            if (length == -1)
                return null;

            var memory = buffer.Slice(position, length).AsMemory();
            var value = memory.ToArray(); // TODO marshal ?
            position += length;

            // TODO compare Spans ???
            if (position >= buffer.Length)
                throw new ProtocolViolationException(Resources.ProtocolViolationInvalidEndChar.Format(position));

            if (!buffer.Slice(position, 2).AsSpan().SequenceEqual(Resp.CRLF))
                throw new ProtocolViolationException(Resources.ProtocolViolationInvalidEndChar.Format(position));

            position += 2;
            return value;
        }

        private static object[] ParseArray(this in ReadOnlySequence<byte> buffer, ref int position)
        {
            var lengthBuffer = ParseInteger(buffer, ref position);
            if (!Utf8Parser.TryParse(lengthBuffer, out int length, out _))
                throw new ProtocolViolationException(Resources.ProtocolViolationParsingInteger.Format(position));

            if (length == -1)
                return null;

            var array = new object[length];
            for (var arrayIndex = 0; arrayIndex < length; arrayIndex++)
                array[arrayIndex] = ParseElement(buffer, ref position).Value;

            return array;
        }
    }
}