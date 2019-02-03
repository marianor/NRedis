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
            // TODO change by a tuple ???
            var bufferState = new BufferState { Buffer = buffer, Position = 0 };
            // TODO it should throw if still elements
            return ParseElement(bufferState);
        }

        // TODO Consider Memory<byte>
        public static IResponse[] Parse(this ReadOnlySequence<byte> buffer, int count)
        {
            var i = 0;
            var responses = new IResponse[count];

            var bufferState = new BufferState { Buffer = buffer, Position = 0 };
            while (buffer.Length > bufferState.Position)
                responses[i++] = ParseElement(bufferState);

            if (i < count || i > count)
                throw new ProtocolViolationException("Invalid responses"); // TODO check messages

            return responses;
        }

        private static IResponse ParseElement(BufferState state)
        {
            var mem = state.Buffer.Slice(state.Position++, 1).GetSpan();
            switch (mem[0])
            {
                case Resp.BulkString:
                    return new StringResponse(DataType.BulkString, ParseBulkString(state));
                case Resp.Integer:
                    return new IntegerResponse(ParseInteger(state));
                case Resp.Error:
                    return new StringResponse(DataType.Error, ParseSimpleString(state));
                case Resp.SimpleString:
                    return new StringResponse(DataType.SimpleString, ParseSimpleString(state));
                case Resp.Array:
                    return new ArrayResponse(ParseArray(state));
                default:
                    throw new ProtocolViolationException(Resources.ProtocolViolationInvalidBeginChar.Format(state.Position - 1));
            }
        }

        private static byte[] ParseSimpleString(BufferState state)
        {
            var buffer = state.Buffer.Slice(state.Position).GetSpan();
            var length = buffer.IndexOf(Resp.CRLF);
            if (length == -1)
                throw new ProtocolViolationException(Resources.ProtocolViolationInvalidEndChar.Format(state.Position));

            state.Position += length + 2;
            return buffer.Slice(0, length).ToArray();
        }

        private static byte[] ParseInteger(BufferState state)
        {
            var buffer = state.Buffer.Slice(state.Position).GetSpan();
            var length = buffer.IndexOf(Resp.CRLF);
            if (length == -1)
                throw new ProtocolViolationException(Resources.ProtocolViolationInvalidEndChar.Format(state.Position));

            state.Position += length + 2;
            return buffer.Slice(0, length).ToArray();
        }

        private static byte[] ParseBulkString(BufferState state)
        {
            var lengthBuffer = ParseInteger(state);
            if (!Utf8Parser.TryParse(lengthBuffer, out int length, out _))
                throw new ProtocolViolationException(Resources.ProtocolViolationParsingInteger.Format(state.Position));

            if (length == -1)
                return null;

            var span = state.Buffer.Slice(state.Position, length).GetSpan();
            var value = span.ToArray(); // TODO marshal ?
            state.Position += length;

            // TODO compare Spans ???
            if (state.Position >= state.Buffer.Length)
                throw new ProtocolViolationException(Resources.ProtocolViolationInvalidEndChar.Format(state.Position));

            var end = state.Buffer.Slice(state.Position, 2).GetSpan();
            if (!end.SequenceEqual(Resp.CRLF))
                throw new ProtocolViolationException(Resources.ProtocolViolationInvalidEndChar.Format(state.Position));

            state.Position += 2;
            return value;
        }

        private static object[] ParseArray(BufferState state)
        {
            var lengthBuffer = ParseInteger(state);
            if (!Utf8Parser.TryParse(lengthBuffer, out int length, out _))
                throw new ProtocolViolationException(Resources.ProtocolViolationParsingInteger.Format(state.Position));

            if (length == -1)
                return null;

            var array = new object[length];
            for (var arrayIndex = 0; arrayIndex < length; arrayIndex++)
                array[arrayIndex] = ParseElement(state).Value;

            return array;
        }

        private class BufferState
        {
            [Obsolete("Remove")]
            public int Position;
            public ReadOnlySequence<byte> Buffer;
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