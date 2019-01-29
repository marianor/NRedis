using Framework.Caching.Redis.Properties;
using System;
using System.Numerics;

namespace Framework.Caching.Redis.Protocol
{
    public class Request : IRequest
    {
        private readonly byte[] _command;
        private readonly object[] _args;

        public Request(string command) : this(command, null)
        {
        }

        public Request(CommandType commandType) : this(commandType, null)
        {
        }

        public Request(string command, params object[] args)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            if (command.Length == 0)
                throw new ArgumentException(Resources.ArgumentCannotBeEmpty, nameof(command));

            _command = Resp.Encoding.GetBytes(command);
            _args = args ?? Array.Empty<object>();
        }

        public Request(CommandType commandType, params object[] args) : this(commandType.ToCommand(), args)
        {
        }

        public string Command => Resp.Encoding.GetString(_command);

        public int Length
        {
            get
            {
                var length = Command.Length + Resp.CRLF.Length;
                foreach (var arg in _args)
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
        }

        public T GetArg<T>(int index)
        {
            return (T)_args[index];
        }

        public object[] GetArgs() => _args;

        public int Write(Memory<byte> buffer)
        {
            var writer = new MemoryWriter(buffer);
            writer.WriteRaw(_command);
            WritePayload(writer);
            writer.WriteRaw(Resp.CRLF);
            return writer.Position;
        }

        private protected virtual void WritePayload(MemoryWriter writer)
        {
            foreach (var arg in _args)
            {
                writer.WriteRaw(Resp.Separator);
                if (arg is byte[] bytesArg)
                    writer.WriteRaw(bytesArg);
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
            }
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
    }
}