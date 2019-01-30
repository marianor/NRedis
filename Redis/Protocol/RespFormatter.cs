using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Framework.Caching.Redis.Protocol
{
    internal class RespFormatter
    {
        public int Format(IRequest request, byte[] buffer)
        {
            // TODO replace by PIPE.IO ?
            Memory<byte> memory = buffer;
            var writer = new MemoryWriter(memory);
            writer.Write(request.Command);
            WritePayload(writer, request.GetArgs());
            writer.Write(Resp.CRLF);
            return writer.Position;
        }

        // TODO Tests ????
        public int Format(IEnumerable<IRequest> requests, byte[] buffer)
        {
            // TODO replace by PIPE.IO ?
            var count = 0;
            Memory<byte> memory = buffer;
            var writer = new MemoryWriter(memory);

            foreach (var request in requests)
            {
                writer.Write(request.Command);
                WritePayload(writer, request.GetArgs());
                writer.Write(Resp.CRLF);
                count++;
            }

            return count;
        }

        internal int GetLength(IRequest request)
        {
            var length = request.Command.Length + Resp.CRLF.Length;
            foreach (var arg in request.GetArgs())
            {
                length++;
                if (arg is byte[] bytesArg)
                    length += bytesArg.Length;
                else if (arg is string stringArg)
                    length += stringArg.Length;
                else if (arg is DateTime dateTimeArg)
                    length += CountDigits(((DateTimeOffset)dateTimeArg).ToUnixTimeMilliseconds());
                else if (arg is DateTimeOffset dateTimeOffsetArg)
                    length += CountDigits(dateTimeOffsetArg.ToUnixTimeMilliseconds());
                else if (arg is TimeSpan timeSpanArg)
                    length += CountDigits((long)timeSpanArg.TotalMilliseconds);
                else if (arg is int intArg)
                    length += CountDigits(intArg);
            }

            return length;
        }

        public int GetLength(IEnumerable<IRequest> requests)
        {
            return requests.Sum(r => GetLength(r));
        }

        private static int CountDigits(BigInteger value)
        {
            var count = value > 0 ? 0 : 1;
            while (value != 0)
            {
                count++;
                value /= 10;
            }

            return count;
        }

        private static void WritePayload(MemoryWriter writer, IEnumerable<object> args)
        {
            foreach (var arg in args)
            {
                writer.Write(Resp.Separator);
                if (arg is byte[] bytesArg)
                    writer.Write(bytesArg);
                else if (arg is string stringArg)
                    writer.Write(stringArg);
                else if (arg is DateTime dateTimeArg)
                    writer.Write(((DateTimeOffset)dateTimeArg).ToUnixTimeMilliseconds());
                else if (arg is DateTimeOffset dateTimeOffsetArg)
                    writer.Write(dateTimeOffsetArg.ToUnixTimeMilliseconds());
                else if (arg is TimeSpan timeSpanArg)
                    writer.Write((long)timeSpanArg.TotalMilliseconds);
                else if (arg is int intArg)
                    writer.Write(intArg);
                else if (arg is long longArg)
                    writer.Write(longArg);
            }
        }
    }
}