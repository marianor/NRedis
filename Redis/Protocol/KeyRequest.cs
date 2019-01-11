using Framework.Caching.Properties;
using System;

namespace Framework.Caching.Protocol
{
    public class KeyRequest : Request
    {
        public KeyRequest(string command, string key) : base(command)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            if (key.Length == 0)
                throw new ArgumentException(Resources.ArgumentCannotBeEmpty, nameof(key));
        }

        public KeyRequest(CommandType commandType, string key) : this(commandType.ToCommand(), key)
        {
        }

        public string Key { get; }

        private protected override void WritePayload(MemoryWriter writer)
        {
            writer.Write(Protocol.Separator);
            writer.Write(Key);
        }
    }
}