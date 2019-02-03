using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Caching.Redis.Protocol
{
    internal static class RespFormatter
    {
        public static ReadOnlySequence<byte> Format(this IRequest request)
        {
            var pipe = new Pipe();
            request.WriteCommandRequest(pipe.Writer);
            pipe.Writer.Complete();

            if (!pipe.Reader.TryRead(out ReadResult result))
                throw new InvalidOperationException(); // TOOD exception

            pipe.Reader.Complete();
            return result.Buffer;
        }

        public static ReadOnlySequence<byte> Format(this IRequest[] requests)
        {
            var pipe = new Pipe();
            requests.WriteCommndsRequest(pipe.Writer);
            pipe.Writer.Complete();

            if (!pipe.Reader.TryRead(out ReadResult result))
                throw new InvalidOperationException(); // TOOD exception

            pipe.Reader.Complete();
            return result.Buffer;
        }

        public static async Task<ReadOnlySequence<byte>> FormatAsync(this IRequest request, CancellationToken token)
        {
            var pipe = new Pipe();
            request.WriteCommandRequest(pipe.Writer);
            pipe.Writer.Complete();

            var result = await pipe.Reader.ReadAsync(token);
            pipe.Reader.Complete();
            return result.Buffer;
        }

        public static async Task<ReadOnlySequence<byte>> FormatAsync(this IRequest[] requests, CancellationToken token)
        {
            var pipe = new Pipe();
            requests.WriteCommndsRequest(pipe.Writer);
            pipe.Writer.Complete();

            var result = await pipe.Reader.ReadAsync(token);
            pipe.Reader.Complete();
            return result.Buffer;
        }

        private static void WriteCommndsRequest(this IRequest[] requests, PipeWriter writer)
        {
            writer.WriteByte(Resp.Array);
            writer.WriteRawInt32(requests.Length);

            foreach (var request in requests)
                request.WriteCommandRequest(writer);

            writer.WriteRaw(Resp.CRLF);
        }

        private static void WriteCommandRequest(this IRequest request, PipeWriter writer)
        {
            writer.WriteByte(Resp.Array);
            writer.WriteRawInt32(request.GetArgs().Length + 1);
            writer.WriteRaw(Resp.CRLF);

            writer.WriteBulkString(request.Command);
            writer.WriteArgs(request.GetArgs());
            writer.WriteRaw(Resp.CRLF);
        }

        private static void WriteByte(this PipeWriter writer, byte value)
        {
            var span = writer.GetSpan(1);
            span[0] = value;
            writer.Advance(1);
        }

        private static void WriteInteger(this PipeWriter writer, int value)
        {
            writer.WriteByte(Resp.Integer);
            writer.WriteRawInt32(value);
            writer.WriteRaw(Resp.CRLF);
        }

        private static void WriteInteger(this PipeWriter writer, long value)
        {
            writer.WriteByte(Resp.Integer);
            writer.WriteRawInt64(value);
            writer.WriteRaw(Resp.CRLF);
        }

        private static void WriteBulkString(this PipeWriter writer, string value)
        {
            writer.WriteBulkString(Resp.Encoding.GetBytes(value));
        }

        private static void WriteBulkString(this PipeWriter writer, byte[] buffer)
        {
            writer.WriteByte(Resp.BulkString);
            writer.WriteRawInt32(buffer.Length);
            writer.WriteRaw(Resp.CRLF);
            writer.WriteRaw(buffer);
            writer.WriteRaw(Resp.CRLF);
        }

        private static void WriteNull(this PipeWriter writer)
        {
            writer.WriteByte(Resp.BulkString);
            writer.WriteRawInt32(-1);
            writer.WriteRaw(Resp.CRLF);
        }

        private static void WriteArgs(this PipeWriter writer, IEnumerable<object> args)
        {
            foreach (var arg in args)
            {
                if (arg == null)
                    writer.WriteNull();
                else if (arg is byte byteArg)
                    writer.WriteBulkString(new[] { byteArg });
                else if (arg is byte[] bytesArg)
                    writer.WriteBulkString(bytesArg);
                else if (arg is string stringArg)
                    writer.WriteBulkString(stringArg);
                else if (arg is DateTime dateTimeArg)
                    writer.WriteInteger(((DateTimeOffset)dateTimeArg).ToUnixTimeMilliseconds());
                else if (arg is DateTimeOffset dateTimeOffsetArg)
                    writer.WriteInteger(dateTimeOffsetArg.ToUnixTimeMilliseconds());
                else if (arg is TimeSpan timeSpanArg)
                    writer.WriteInteger((long)timeSpanArg.TotalMilliseconds);
                else if (arg is int intArg)
                    writer.WriteInteger(intArg);
                else if (arg is long longArg)
                    writer.WriteInteger(longArg);
                else
                    throw new InvalidOperationException(); // TODO exception
            }
        }

        private static void WriteRaw(this PipeWriter writer, byte[] buffer)
        {
            var span = writer.GetSpan(buffer.Length);
            buffer.CopyTo(span);
            writer.Advance(buffer.Length);
        }

        private static void WriteRawInt32(this PipeWriter writer, int value)
        {
            var span = writer.GetSpan(11);
            if (!Utf8Formatter.TryFormat(value, span, out int written))
                throw new InvalidOperationException(); // TODO exception
            writer.Advance(written);
        }

        private static void WriteRawInt64(this PipeWriter writer, long value)
        {
            var span = writer.GetSpan(21);
            if (!Utf8Formatter.TryFormat(value, span, out int written))
                throw new InvalidOperationException(); // TODO exception
            writer.Advance(written);
        }
    }
}