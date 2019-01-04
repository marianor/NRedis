namespace Framework.Caching.Protocol
{
    internal class RespProtocol
    {
        public const byte CR = (byte)'\r';
        public const byte LF = (byte)'\n';
        public static readonly byte[] CRLF = { CR, LF };
        public const string Success = "OK";
    }
}