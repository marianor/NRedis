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
            var index = 0;
            var span = buffer.Span;
            foreach (var b in Encoding.UTF8.GetBytes(Command))
                span[index++] = b;

            span[index++] = RespClient.CR;
            span[index++] = RespClient.LF;
            return index;
        }
    }
}