using Framework.Caching.Properties;
using Framework.Caching.Transport;
using System;
using System.IO.Pipelines;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Caching.Protocol
{
    // Ver http://redis.io/topics/protocol
    public class RespClient : IRespClient
    {
        internal const byte CR = (byte)'\r';
        internal const byte LF = (byte)'\n';

        private readonly ITransport _transport;
        private readonly ITransportSettings _settings;

        public RespClient(ITransportSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _transport = settings.CreateTransport();
        }

        private void Connect()
        {
            if (_transport.State == TransportState.Closed)
            {
                _transport.Connect();
                if (_settings.Password != null)
                {
                    var response = Execute(new KeyRequest(CommandType.Auth, _settings.Password));
                    VerifyConnection(response);
                }
            }
        }

        private async Task ConnectAsync(CancellationToken token)
        {
            if (_transport.State == TransportState.Closed)
            {
                await _transport.ConnectAsync(token).ConfigureAwait(false);
                if (_settings.Password != null)
                {
                    var response = await ExecuteAsync(new[] { new KeyRequest(CommandType.Auth, _settings.Password) }, token).ConfigureAwait(false);
                    VerifyConnection(response);
                }
            }
        }

        public IResponse[] Execute(params IRequest[] requests)
        {
            if (requests == null)
                throw new ArgumentNullException(nameof(requests));
            if (requests.Length == 0)
                throw new ArgumentException(Resources.ArgumentCannotBeEmpty, nameof(requests));

            Connect();
            // TODO make better
            var requestsBuffer = GetBufferAsync(requests).Result;
            var responseText = _transport.Send(requestsBuffer);
            return new RespParser(responseText).Parse().ToArray();
        }

        public async Task<IResponse[]> ExecuteAsync(IRequest[] requests, CancellationToken token = default(CancellationToken))
        {
            if (requests == null)
                throw new ArgumentNullException(nameof(requests));
            if (requests.Length == 0)
                throw new ArgumentException(Resources.ArgumentCannotBeEmpty, nameof(requests));

            await ConnectAsync(token).ConfigureAwait(false);
            // TODO make better
            var requestsBuffer = await GetBufferAsync(requests);
            var responseText = await _transport.SendAsync(requestsBuffer, token).ConfigureAwait(false);
            return new RespParser(responseText).Parse().ToArray();
        }

        private async static ValueTask<byte[]> GetBufferAsync(IRequest[] requests)
        {
            // TODO improve
            var size = 0;
            var pipe = new Pipe();
            var memory = pipe.Writer.GetMemory(4096);
            foreach (var request in requests)
            {
                var requestBuffer = request.Buffer;
                size += requestBuffer.Length;
                await pipe.Writer.WriteAsync(requestBuffer);
            }

            await pipe.Writer.FlushAsync();
            pipe.Writer.Complete();
            return memory.Slice(0, size).ToArray();
        }

        private static void VerifyConnection(IResponse[] response)
        {
            if (!Equals(response[0].Value, "OK")) // TODO Remove harcoded value
                throw new AuthenticationException(""); // TODO make a clear message about exception
        }
    }
}