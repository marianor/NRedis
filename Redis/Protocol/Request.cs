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

        public virtual Memory<byte> Buffer => Encoding.UTF8.GetBytes(Command + (char)RespClient.CR + (char)RespClient.LF);

        public string Command { get; }
    }
}