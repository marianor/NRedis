using Microsoft.Extensions.Logging;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;

namespace NRedis.Transport
{
    public interface ITransport
    {
        ILogger Logger { get; set; }

        TransportState State { get; }

        void Connect();

        Task ConnectAsync(CancellationToken token);

        ReadOnlySequence<byte> Send(ReadOnlySequence<byte> request);

        Task<ReadOnlySequence<byte>> SendAsync(ReadOnlySequence<byte> request, CancellationToken token);
    }
}