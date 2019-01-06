using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Caching.Protocol
{
    public interface IRespClient
    {
        IResponse Execute(IRequest request);

        IEnumerable<IResponse> Execute(IEnumerable<IRequest> requests);

        Task<IResponse> ExecuteAsync(IRequest request, CancellationToken token);

        Task<IEnumerable<IResponse>> ExecuteAsync(IEnumerable<IRequest> requests, CancellationToken token);
    }
}