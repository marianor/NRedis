using System.Text;

namespace NRedis.Protocol
{
    internal class Resp
    {
        public static readonly byte[] Success = Encoding.UTF8.GetBytes("OK");

        public static readonly byte[] CRLF = Encoding.UTF8.GetBytes("\r\n");

        public const byte SimpleString = (byte)'+';

        public const byte Error = (byte)'-';

        public const byte Integer = (byte)':';

        public const byte BulkString = (byte)'$';

        public const byte Array = (byte)'*';
    }
}