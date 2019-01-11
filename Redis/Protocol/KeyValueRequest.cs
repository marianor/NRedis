namespace Framework.Caching.Protocol
{
    public class KeyValueRequest : KeyRequest
    {
        private readonly byte[] _value;

        public KeyValueRequest(string command, string key, byte[] value) : base(command, key)
        {
            _value = value;
        }

        public KeyValueRequest(string command, string key, string value) : this(command, key, Protocol.Encoding.GetBytes(value))
        {
        }

        public KeyValueRequest(CommandType commandType, string key, byte[] value) : this(commandType.ToCommand(), key, value)
        {
        }

        public KeyValueRequest(CommandType commandType, string key, string value) : this(commandType.ToCommand(), key, value)
        {
        }

        public string Value => Protocol.Encoding.GetString(_value);

        private protected override void WritePayload(MemoryWriter writer)
        {
            base.WritePayload(writer);
            writer.Write(Protocol.Separator);
            writer.Write(_value);
        }
    }
}