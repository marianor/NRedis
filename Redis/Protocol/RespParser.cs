using Framework.Caching.Redis.Properties;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Net;

namespace Framework.Caching.Redis.Protocol
{
    internal class RespParser
    {
        private const byte SimpleString = (byte)'+';
        private const byte Error = (byte)'-';
        private const byte Integer = (byte)':';
        private const byte BulkString = (byte)'$';
        private const byte Array = (byte)'*';

        // TODO Consider Memory<byte>
        public IEnumerable<IResponse> Parse(byte[] buffer)
        {
            var bufferState = new BufferState { Buffer = buffer, Position = 0 };
            while (buffer.Length > bufferState.Position)
                yield return ParseElement(bufferState);
        }

        private static IResponse ParseElement(BufferState state)
        {
            switch (state.Buffer[state.Position++])
            {
                case SimpleString:
                    return new StringResponse(DataType.SimpleString, ParseSimpleString(state));
                case Error:
                    return new StringResponse(DataType.Error, ParseSimpleString(state));
                case Integer:
                    return new IntegerResponse(ParseInteger(state));
                case BulkString:
                    return new StringResponse(DataType.BulkString, ParseBulkString(state));
                case Array:
                    return new ArrayResponse(ParseArray(state));
                default:
                    throw new ProtocolViolationException(Resources.ProtocolViolationInvalidBeginChar.Format(state.Position - 1));
            }
        }

        private static byte[] ParseSimpleString(BufferState state)
        {
            var buffer = state.Buffer.AsSpan(state.Position);
            var length = buffer.IndexOf(RespProtocol.CRLF);
            if (length == -1)
                throw new ProtocolViolationException(Resources.ProtocolViolationInvalidEndChar.Format(state.Position));

            state.Position += length + 2;
            return buffer.Slice(0, length).ToArray();
        }

        private static byte[] ParseInteger(BufferState state)
        {
            var buffer = state.Buffer.AsSpan(state.Position);
            var length = buffer.IndexOf(RespProtocol.CRLF);
            if (length == -1)
                throw new ProtocolViolationException(Resources.ProtocolViolationInvalidEndChar.Format(state.Position));

            state.Position += length + 2;
            return buffer.Slice(0, length).ToArray();
        }

        private static byte[] ParseBulkString(BufferState state)
        {
            var lengthBuffer = ParseInteger(state);
            if (!Utf8Parser.TryParse(lengthBuffer, out int length, out int bytesConsumed))
                throw new ProtocolViolationException(Resources.ProtocolViolationParsingInteger.Format(state.Position));

            if (length == -1)
                return null;

            var value = state.Buffer.AsSpan().Slice(state.Position, length).ToArray();
            state.Position += length;
            // TODO compare Spans ???
            if (state.Position >= state.Buffer.Length || state.Buffer[state.Position] != RespProtocol.CRLF[0] || state.Buffer[state.Position + 1] != RespProtocol.CRLF[1])
                throw new ProtocolViolationException(Resources.ProtocolViolationInvalidEndChar.Format(state.Position));

            state.Position += 2;
            return value;
        }

        private static object[] ParseArray(BufferState state)
        {
            var lengthBuffer = ParseInteger(state);
            if (!Utf8Parser.TryParse(lengthBuffer, out int length, out int bytesConsumed))
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