using Framework.Caching.Redis.Properties;
using Framework.Caching.Redis.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Caching.Redis.Protocol
{
    public class RespClient : IRespClient
    {
        private readonly RedisCacheOptions _optionsAccessor;
        private readonly ITransport _transport;

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
                    VerifyAuthentication(response);
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
                    VerifyAuthentication(response);
                }
            }
        }

        public IResponse Execute(IRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            Connect();
            var input = request.Format();
            var output = _transport.Send(input);
            return output.Parse(); // TODO chenge
        }

        public IResponse[] Execute(IRequest[] requests)
        {
            if (requests == null)
                throw new ArgumentNullException(nameof(requests));
            if (requests.Length == 0)
                throw new ArgumentException(Resources.ArgumentCannotBeEmpty, nameof(requests));

            Connect();
            var input = requests.Format();
            var output = _transport.Send(input);
            return output.Parse((int)output.Length);
        }

        public async Task<IResponse> ExecuteAsync(IRequest request, CancellationToken token = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            await ConnectAsync(token).ConfigureAwait(false);
            var input = await request.FormatAsync(token).ConfigureAwait(false);
            var output = await _transport.SendAsync(input, token).ConfigureAwait(false);
            return output.Parse();
        }

        public async Task<IResponse[]> ExecuteAsync(IRequest[] requests, CancellationToken token = default)
        {
            if (requests == null)
                throw new ArgumentNullException(nameof(requests));
            if (requests.Length == 0)
                throw new ArgumentException(Resources.ArgumentCannotBeEmpty, nameof(requests));

            await ConnectAsync(token).ConfigureAwait(false);
            var input = await requests.FormatAsync(token).ConfigureAwait(false);
            var output = await _transport.SendAsync(input, token).ConfigureAwait(false);
            return output.Parse((int)output.Length);
        }

        private static void VerifyAuthentication(IResponse response)
        {
            if (!Equals(response.Value, Resp.Success))
                throw new AuthenticationException(""); // TODO make a clear message about exception
        }
    }
}