using Framework.Caching.Redis.Properties;
using System;

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

            _command = RespProtocol.Encoding.GetBytes(command);
            _args = args;
        }

        public Request(CommandType commandType, params object[] args) : this(commandType.ToCommand(), args)
        {
        }

        public string Command => RespProtocol.Encoding.GetString(_command);

        public T GetArg<T>(int index)
        {
            return (T)_args[index];
        }

        public object[] GetArgs() => _args;

        public int Write(Memory<byte> buffer)
        {
            var writer = new MemoryWriter(buffer);
            writer.Write(_command);
            if (_args != null)
                WritePayload(writer);
            writer.Write(RespProtocol.CRLF);
            return writer.Position;
        }

        private protected virtual void WritePayload(MemoryWriter writer)
        {
            foreach (var arg in _args)
            {
                writer.Write(RespProtocol.Separator);
                if (arg is byte[] bytesArg)
                    writer.Write(bytesArg);
                else if (arg is string stringArg)
                    writer.Write(stringArg);
            }
        }
    }
}