using Framework.Caching.Properties;
using System;

namespace Framework.Caching.Protocol
{
    public class Request : IRequest
    {
        private readonly byte[] _command;

        public Request(string command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            if (command.Length == 0)
                throw new ArgumentException(Resources.ArgumentCannotBeEmpty, nameof(command));

            _command = Protocol.Encoding.GetBytes(command);
        }

        public Request(CommandType commandType) : this(commandType.ToCommand())
        {
        }

        public string Command => Protocol.Encoding.GetString(_command);

        public int Write(Memory<byte> buffer)
        {
            var writer = new MemoryWriter(buffer);
            writer.Write(_command);
            WritePayload(writer);
            writer.Write(Protocol.CRLF);
            return writer.Position;
        }

        private protected virtual void WritePayload(MemoryWriter writer)
        {
        }
    }
}