using System.Threading;
using System.Threading.Tasks;

namespace Framework.Caching.Protocol
{
    public interface IRespClient
    {
        IResponse[] Execute(params IRequest[] requests);

        Task<IResponse[]> ExecuteAsync(IRequest[] requests, CancellationToken token);
    }
}