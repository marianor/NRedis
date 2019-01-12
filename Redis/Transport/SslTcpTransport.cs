using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Framework.Caching.Redis.Transport
{
    public class SslTcpTransport : TcpTransport
    {
        public SslTcpTransport(string host, int port) : base(host, port)
        {
        }

        protected override Stream GetStream(TcpClient client)
        {
            var stream = new SslStream(base.GetStream(client), false);
            stream.AuthenticateAsClient(Host);
            return stream;
        }

        protected override async Task<Stream> GetStreamAsync(TcpClient client)
        {
            var stream = new SslStream(base.GetStream(client), false);
            await stream.AuthenticateAsClientAsync(Host).ConfigureAwait(false);
            return stream;
        }
    }
}