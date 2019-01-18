using Framework.Caching.Redis.Properties;
using Framework.Caching.Redis.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Caching.Redis.Protocol
{
    // Ver http://redis.io/topics/protocol
    public class RespClient : IRespClient
    {
        private readonly RedisCacheOptions _optionsAccessor;
        private readonly ITransport _transport;
        // TODO should I use a Pool here ?
        //private static readonly ArrayPool<byte> _pool = ArrayPool<byte>.Create();

        public RespClient(IOptions<RedisCacheOptions> optionsAccessor, ITransport transport = null)
        {
            _optionsAccessor = optionsAccessor?.Value ?? throw new ArgumentNullException(nameof(optionsAccessor));
            _transport = transport ?? (_optionsAccessor.UseSsl ? new SslTcpTransport(_optionsAccessor.Host, _optionsAccessor.Port) : new TcpTransport(_optionsAccessor.Host, _optionsAccessor.Port));
            _transport.Logger = optionsAccessor.Value.LoggerFactory?.CreateLogger(_transport.GetType());
        }

        private void Connect()
        {
            if (_transport.State == TransportState.Closed)
            {
                _transport.Connect();
                if (_optionsAccessor.Password != null)
                {
                    var response = Execute(new Request(CommandType.Auth, _optionsAccessor.Password));
                    VerifyConnection(response);
                }
            }
        }

        private async Task ConnectAsync(CancellationToken token)
        {
            if (_transport.State == TransportState.Closed)
            {
                await _transport.ConnectAsync(token).ConfigureAwait(false);
                if (_optionsAccessor.Password != null)
                {
                    var response = await ExecuteAsync(new Request(CommandType.Auth, _optionsAccessor.Password), token).ConfigureAwait(false);
                    VerifyConnection(response);
                }
            }
        }

        public IResponse Execute(IRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            Connect();
            // TODO check
            var requestsBuffer = GetBuffer(request);
            var responseText = _transport.Send(requestsBuffer);
            return new RespParser().Parse(responseText).Single();
        }

        public IEnumerable<IResponse> Execute(IEnumerable<IRequest> requests)
        {
            if (requests == null)
                throw new ArgumentNullException(nameof(requests));
            if (!requests.Any())
                throw new ArgumentException(Resources.ArgumentCannotBeEmpty, nameof(requests));

            Connect();
            // TODO check
            var requestsBuffer = GetBuffer(requests);
            var responseText = _transport.Send(requestsBuffer);
            return new RespParser().Parse(responseText);
        }

        public async Task<IResponse> ExecuteAsync(IRequest request, CancellationToken token = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            await ConnectAsync(token).ConfigureAwait(false);
            // TODO make better
            var requestsBuffer = GetBuffer(request);
            var responseText = await _transport.SendAsync(requestsBuffer, token).ConfigureAwait(false);
            return new RespParser().Parse(responseText).Single();
        }

        public async Task<IEnumerable<IResponse>> ExecuteAsync(IEnumerable<IRequest> requests, CancellationToken token = default)
        {
            if (requests == null)
                throw new ArgumentNullException(nameof(requests));
            if (!requests.Any())
                throw new ArgumentException(Resources.ArgumentCannotBeEmpty, nameof(requests));

            await ConnectAsync(token).ConfigureAwait(false);
            // TODO make better
            var requestsBuffer = GetBuffer(requests);
            var responseText = await _transport.SendAsync(requestsBuffer, token).ConfigureAwait(false);
            return new RespParser().Parse(responseText);
        }

        // TODO maybe extension method
        private static byte[] GetBuffer(IRequest request)
        {
            // TODO use pool ?
            Memory<byte> memory = new byte[4096];
            var length = request.Write(memory);
            return memory.Slice(0, length).ToArray();
        }

        // TODO maybe extension method
        private static byte[] GetBuffer(IEnumerable<IRequest> requests)
        {
            // TODO use pool ?
            Memory<byte> memory = new byte[4096];
            var length = 0;
            foreach (var request in requests)
                length += request.Write(memory.Slice(length));

            return memory.Slice(0, length).ToArray();
        }

        private static void VerifyConnection(IResponse response)
        {
            if (!Equals(response.Value, RespProtocol.Success))
                throw new AuthenticationException(""); // TODO make a clear message about exception
        }
    }
}