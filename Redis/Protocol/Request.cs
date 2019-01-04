using Framework.Caching.Properties;
using System;
using System.Text;

namespace Framework.Caching.Protocol
{
    public class Request : IRequest
    {
        public Request(string command)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
            if (command.Length == 0)
                throw new ArgumentException(Resources.ArgumentCannotBeEmpty, nameof(command));
        }

        public Request(CommandType commandType) : this(commandType.ToCommand())
        {
        }

        public string Command { get; }

        public virtual int Write(Memory<byte> buffer)
        {
            var writer = new MemoryWriter(buffer);
            writer.Write(Command);
            writer.Write(RespProtocol.CRLF);
            return writer.Position;
        }
    }
}