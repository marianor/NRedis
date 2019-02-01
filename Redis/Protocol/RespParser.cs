using Framework.Caching.Redis.Properties;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Net;

namespace Framework.Caching.Redis.Protocol
{
    internal static class RespParser
    {
        // TODO Consider Memory<byte>
        public static IResponse Parse(this byte[] buffer)
        {
            // TODO change by a tuple ???
            var bufferState = new BufferState { Buffer = buffer, Position = 0 };
            // TODO it should throw if still elements
            return ParseElement(bufferState);
        }

        // TODO Consider Memory<byte>
        public static IResponse[] Parse(this byte[] buffer, int count)
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
            switch (state.Buffer[state.Position++])
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
            var buffer = state.Buffer.AsSpan(state.Position);
            var length = buffer.IndexOf(Resp.CRLF);
            if (length == -1)
                throw new ProtocolViolationException(Resources.ProtocolViolationInvalidEndChar.Format(state.Position));

            state.Position += length + 2;
            return buffer.Slice(0, length).ToArray();
        }

        private static byte[] ParseInteger(BufferState state)
        {
            var buffer = state.Buffer.AsSpan(state.Position);
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

            var value = state.Buffer.AsSpan().Slice(state.Position, length).ToArray();
            state.Position += length;
            // TODO compare Spans ???
            if (state.Position >= state.Buffer.Length || state.Buffer[state.Position] != Resp.CRLF[0] || state.Buffer[state.Position + 1] != Resp.CRLF[1])
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
            public byte[] Buffer;
            public int Position;
        }
    }
}