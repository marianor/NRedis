using System.Threading;
using System.Threading.Tasks;

namespace Framework.Caching.Transport
{
    public interface ITransport
    {
        TransportState State { get; }

        void Connect();

        Task ConnectAsync(CancellationToken token);

        string Send(string request);

        Task<string> SendAsync(string request, CancellationToken token);
    }
}