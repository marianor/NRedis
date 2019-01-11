using System.Text;

namespace Framework.Caching.Protocol
{
    internal class Protocol
    {
        public static readonly Encoding Encoding = Encoding.UTF8;

        public const string Success = "OK";

        public static readonly byte[] Separator = Encoding.GetBytes(" ");
        public static readonly byte[] CRLF = Encoding.GetBytes("\r\n");
    }
}