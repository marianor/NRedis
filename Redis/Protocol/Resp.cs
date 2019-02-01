using System;
using System.Text;

namespace Framework.Caching.Redis.Protocol
{
    internal class Resp
    {
        public static readonly Encoding Encoding = Encoding.UTF8;

        public const string Success = "OK";

        public static readonly byte[] CRLF = Encoding.GetBytes("\r\n");

        public const byte SimpleString = (byte)'+';
        public const byte Error = (byte)'-';
        public const byte Integer = (byte)':';
        public const byte BulkString = (byte)'$';
        public const byte Array = (byte)'*';
    }
}