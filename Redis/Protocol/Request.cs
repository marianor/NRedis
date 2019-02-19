using NRedis.Properties;
using System;

namespace NRedis.Protocol
{
    public class Request : IRequest
    {
        private readonly object[] _args;

        public Request(string command) : this(command, null)
        {
        }

        public Request(CommandType commandType) : this(commandType, null)
        {
        }

        public Request(string command, params object[] args)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
            if (command.Length == 0)
                throw new ArgumentException(Resources.ArgumentCannotBeEmpty, nameof(command));

            _args = args ?? Array.Empty<object>();
        }

        public Request(CommandType commandType, params object[] args) : this(commandType.ToCommand(), args)
        {
        }

        public string Command { get; }

        public T GetArg<T>(int index) => (T)_args[index];

        public object[] GetArgs() => _args;
    }
}