using System.Threading;
using System.Threading.Tasks;

namespace Framework.Caching.Transport
{
    public interface ITransport
    {
        TransportState State { get; }

        void Connect();

        Task ConnectAsync(CancellationToken token);

        byte[] Send(byte[] request);

        Task<byte[]> SendAsync(byte[] request, CancellationToken token);
    }
}