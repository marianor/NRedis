using System;
using System.Collections.Generic;
using System.Linq;

namespace Framework.Caching.Redis.Protocol
{
    internal static class RespFormatter
    {
        public static int Format(this IRequest request, byte[] buffer)
        {
            // TODO replace by PIPE.IO ?
            Memory<byte> memory = buffer;
            var writer = new MemoryWriter(memory);
            return WriteCommandRequest(request, writer);
        }

        private static int WriteCommandRequest(IRequest request, MemoryWriter writer)
        {
            writer.Write(Resp.Array);
            writer.Write(request.GetArgs().Length + 1);
            writer.Write(Resp.CRLF);

            WriteBulkString(writer, request.Command);
            WriteArgs(writer, request.GetArgs());
            writer.Write(Resp.CRLF);
            return writer.Position;
        }

        // TODO Tests ????
        public static int Format(this IEnumerable<IRequest> requests, byte[] buffer)
        {
            // TODO replace by PIPE.IO ?
            var count = 0;
            Memory<byte> memory = buffer;
            var writer = new MemoryWriter(memory);

            writer.Write(Resp.Array);
            writer.Write(requests.Count());
            writer.Write(Resp.CRLF);

            foreach (var request in requests)
            {
                WriteCommandRequest(request, writer);
                count++;
            }

            writer.Write(Resp.CRLF);

            return count;
        }

        internal static int GetLength(this IRequest request)
        {
            var endLength = Resp.CRLF.Length;
            int length = CountDigits(request.GetArgs().Length + 1) + 3 + GetBulkStringLength(request.Command.Length) + endLength;
            foreach (var arg in request.GetArgs())
            {
                if (arg is byte[] bytesArg)
                    length += GetBulkStringLength(bytesArg.Length);
                else if (arg is string stringArg)
                    length += GetBulkStringLength(stringArg.Length);
                else if (arg is DateTime dateTimeArg)
                    length += CountDigits(((DateTimeOffset)dateTimeArg).ToUnixTimeMilliseconds()) + endLength + 1;
                else if (arg is DateTimeOffset dateTimeOffsetArg)
                    length += CountDigits(dateTimeOffsetArg.ToUnixTimeMilliseconds()) + endLength + 1;
                else if (arg is TimeSpan timeSpanArg)
                    length += CountDigits((long)timeSpanArg.TotalMilliseconds) + endLength + 1;
                else if (arg is int intArg)
                    length += CountDigits(intArg) + endLength + 1;
                else if (arg is byte byteArg)
                    length += CountDigits(byteArg) + endLength + 1;
            }

            return length;
        }

        private static int GetBulkStringLength(int length)
        {
            return length + CountDigits(length) + Resp.CRLF.Length * 2 + 1;
        }

        public static int GetLength(this IEnumerable<IRequest> requests)
        {
            return requests.Sum(r => GetLength(r)) + CountDigits(requests.Count()) + Resp.CRLF.Length + 1;
        }

        private static int CountDigits(long value)
        {
            if (value == 0)
                return 1;
            return (int)(Math.Log10(value) + 1);
        }

        private static void WriteArgs(MemoryWriter writer, IEnumerable<object> args)
        {
            foreach (var arg in args)
            {
                //writer.Write(Resp.Separator);
                if (arg == null)
                    WriteNull(writer);
                if (arg is byte[] bytesArg)
                    WriteBulkString(writer, bytesArg);
                else if (arg is string stringArg)
                    WriteBulkString(writer, stringArg);
                else if (arg is DateTime dateTimeArg)
                    WriteInteger(writer, ((DateTimeOffset)dateTimeArg).ToUnixTimeMilliseconds());
                else if (arg is DateTimeOffset dateTimeOffsetArg)
                    WriteInteger(writer, dateTimeOffsetArg.ToUnixTimeMilliseconds());
                else if (arg is TimeSpan timeSpanArg)
                    WriteInteger(writer, (long)timeSpanArg.TotalMilliseconds);
                else if (arg is int intArg)
                    WriteInteger(writer, intArg);
                else if (arg is long longArg)
                    WriteInteger(writer, longArg);
                if (arg is byte byteArg)
                    WriteBulkString(writer, new[] { byteArg });
            }
        }

        private static void WriteBulkString(MemoryWriter writer, string stringArg)
        {
            WriteBulkString(writer, Resp.Encoding.GetBytes(stringArg));
        }

        private static void WriteNull(MemoryWriter writer)
        {
            writer.Write(Resp.BulkString);
            writer.Write(-1);
            writer.Write(Resp.CRLF);
        }

        private static void WriteBulkString(MemoryWriter writer, byte[] bytesArg)
        {
            writer.Write(Resp.BulkString);
            writer.Write(bytesArg.Length);
            writer.Write(Resp.CRLF);
            writer.Write(bytesArg);
            writer.Write(Resp.CRLF);
        }

        private static void WriteInteger(MemoryWriter writer, long dateTimeArg)
        {
            writer.Write(Resp.Integer);
            writer.Write(dateTimeArg);
            writer.Write(Resp.CRLF);
        }
    }
}