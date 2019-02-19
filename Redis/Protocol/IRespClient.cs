using System.Threading;
using System.Threading.Tasks;

namespace NRedis.Protocol
{
    public interface IRespClient
    {
        IResponse Execute(IRequest request);

        IResponse[] Execute(IRequest[] requests);

        Task<IResponse> ExecuteAsync(IRequest request, CancellationToken token);

        Task<IResponse[]> ExecuteAsync(IRequest[] requests, CancellationToken token);
    }
}