using System;
using System.Text;

namespace Framework.Caching.Protocol
{
    public class KeyValueRequest : KeyRequest
    {
        public KeyValueRequest(string command, string key, string value) : base(command, key)
        {
            Value = value;
        }

        public KeyValueRequest(CommandType commandType, string key, string value) : this(commandType.ToCommand(), key, value)
        {
        }

        public string Value { get; }

        public override Memory<byte> Buffer => Encoding.UTF8.GetBytes(Command + " " + Key + " " + Value + (char)RespClient.CR + (char)RespClient.LF);
    }
}