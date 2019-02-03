using Framework.Caching.Redis.Properties;
using System;
using System.Buffers;
using System.Buffers.Text;
using System.Net;

namespace Framework.Caching.Redis.Protocol
{
    internal static class RespParser
    {
        public static IResponse Parse(this ReadOnlySequence<byte> buffer)
        {
            var position = 0;
            // TODO it should throw if still elements
            return ParseElement(buffer, ref position);
        }

        // TODO Consider Memory<byte>
        public static IResponse[] Parse(this ReadOnlySequence<byte> buffer, int count)
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

        private static IResponse ParseElement(this ReadOnlySequence<byte> buffer, ref int position)
        {
            var span = buffer.Slice(position++, 1).GetSpan();
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

        private static byte[] ParseSimpleString(this ReadOnlySequence<byte> buffer, ref int position)
        {
            var span = buffer.Slice(position).GetSpan();
            var length = span.IndexOf(Resp.CRLF);
            if (length == -1)
                throw new ProtocolViolationException(Resources.ProtocolViolationInvalidEndChar.Format(position));

            position += length + 2;
            return span.Slice(0, length).ToArray(); // TODO MemoryMarshal
        }

        private static byte[] ParseInteger(this ReadOnlySequence<byte> buffer, ref int position)
        {
            var span = buffer.Slice(position).GetSpan();
            var length = span.IndexOf(Resp.CRLF);
            if (length == -1)
                throw new ProtocolViolationException(Resources.ProtocolViolationInvalidEndChar.Format(position));

            position += length + 2;
            return span.Slice(0, length).ToArray();
        }

        private static byte[] ParseBulkString(this ReadOnlySequence<byte> buffer, ref int position)
        {
            var lengthBuffer = ParseInteger(buffer, ref position);
            if (!Utf8Parser.TryParse(lengthBuffer, out int length, out _))
                throw new ProtocolViolationException(Resources.ProtocolViolationParsingInteger.Format(position));

            if (length == -1)
                return null;

            var span = buffer.Slice(position, length).GetSpan();
            var value = span.ToArray(); // TODO marshal ?
            position += length;

            // TODO compare Spans ???
            if (position >= buffer.Length)
                throw new ProtocolViolationException(Resources.ProtocolViolationInvalidEndChar.Format(position));

            var end = buffer.Slice(position, 2).GetSpan();
            if (!end.SequenceEqual(Resp.CRLF))
                throw new ProtocolViolationException(Resources.ProtocolViolationInvalidEndChar.Format(position));

            position += 2;
            return value;
        }

        private static object[] ParseArray(this ReadOnlySequence<byte> buffer, ref int position)
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

        // TODO send to extensions ???
        private static ReadOnlySpan<byte> GetSpan(this ReadOnlySequence<byte> buffer)
        {
            var position = buffer.Start;
            if (!buffer.TryGet(ref position, out ReadOnlyMemory<byte> memory))
                throw new ProtocolViolationException(); // TODO exception

            return memory.Span;
        }
    }
}