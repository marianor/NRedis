﻿using Framework.Caching.Properties;
using Framework.Caching.Transport;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Caching.Protocol
{
    // Ver http://redis.io/topics/protocol
    public class RespClient : IRespClient
    {
        private readonly ITransport _transport;
        private readonly ITransportSettings _settings;
        // TODO should I use a Pool here ?
        //private static readonly ArrayPool<byte> _pool = ArrayPool<byte>.Create();

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
                    var response = await ExecuteAsync(new KeyRequest(CommandType.Auth, _settings.Password), token).ConfigureAwait(false);
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
            var length =  request.Write(memory);
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