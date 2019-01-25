using System.Text;

namespace Framework.Caching.Redis.Protocol
{
    internal class Resp
    {
        public static readonly Encoding Encoding = Encoding.UTF8;

        public const string Success = "OK";

        public const byte Separator = (byte)' ';
        public static readonly byte[] CRLF = Encoding.GetBytes("\r\n");
    }
}