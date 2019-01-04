using System.Text;

namespace Framework.Caching.Protocol
{
    internal class RespProtocol
    {
        public const string Success = "OK";

        public static readonly byte[] Separator = Encoding.UTF8.GetBytes(" ");
        public static readonly byte[] CRLF = Encoding.UTF8.GetBytes("\r\n");
    }
}