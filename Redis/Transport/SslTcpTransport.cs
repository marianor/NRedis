using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Framework.Caching.Transport
{
    public class SslTcpTransport : TcpTransport
    {
        public SslTcpTransport(string host, int port) : base(host, port)
        {
        }

        protected override Stream GetStream(TcpClient client)
        {
            var stream = base.GetStream(client);
            var sslStream = new SslStream(stream, false);
            sslStream.AuthenticateAsClient(Host);
            return sslStream;
        }

        protected override async Task<Stream> GetStreamAsync(TcpClient client)
        {
            var stream = base.GetStream(client);
            var sslStream = new SslStream(stream, false);
            await sslStream.AuthenticateAsClientAsync(Host);
            return sslStream;
        }
    }
}