using Framework.Caching.Properties;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Caching.Transport
{
    public class TcpTransport : ITransport, IDisposable
    {
        private const int DefaultCapacity = 4096;

        private TcpClient _client;
        private Stream _stream;

        public TcpTransport(string host, int port)
        {
            Host = host ?? throw new ArgumentNullException(nameof(host));
            if (host.Length == 0)
                throw new ArgumentException(Resources.ArgumentCannotBeEmpty, nameof(host));
            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
                throw new ArgumentOutOfRangeException(nameof(port));
            Port = port;
        }

        public ILogger Logger { get; set; }

        public TransportState State { get; private set; }

        public string Host { get; }

        public int Port { get; }

        public void Connect()
        {
            _client = new TcpClient();
            _client.Connect(Host, Port);
            _stream = GetStream(_client);
            State = TransportState.Connected;
        }

        public async Task ConnectAsync(CancellationToken token = default(CancellationToken))
        {
            _client = new TcpClient();
            await _client.ConnectAsync(Host, Port).ConfigureAwait(false);
            _stream = await GetStreamAsync(_client).ConfigureAwait(false);
            State = TransportState.Connected;
        }

        protected virtual Stream GetStream(TcpClient client) => client.GetStream();

        protected virtual async Task<Stream> GetStreamAsync(TcpClient client) => await Task.FromResult(client.GetStream()).ConfigureAwait(false);

        public byte[] Send(byte[] request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            Logger?.LogTrace($"[Request]\r\n{Log(request)}");
            _stream.Write(request, 0, request.Length);

            var response = ReadResponse();
            Logger?.LogTrace($"[Response]\r\n{Log(response)}");
            return response;
        }

        public async Task<byte[]> SendAsync(byte[] request, CancellationToken token = default(CancellationToken))
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            Logger?.LogTrace($"[Request]\r\n{Log(request)}");
            await _stream.WriteAsync(request, 0, request.Length).ConfigureAwait(false);

            var response = await ReadResponseAsync(token).ConfigureAwait(false);
            Logger?.LogTrace($"[Response]\r\n{Log(response)}");
            return response;
        }

        private byte[] ReadResponse()
        {
            // TODO change by PipeReader
            using (var output = new MemoryStream(DefaultCapacity))
            {
                var buffer = new byte[DefaultCapacity];
                var bytes = _stream.Read(buffer, 0, buffer.Length);
                output.WriteAsync(buffer, 0, bytes);
                while (_client.Available != 0)
                {
                    bytes = _stream.Read(buffer, 0, buffer.Length);
                    output.WriteAsync(buffer, 0, bytes);
                }

                return output.ToArray();
            }
        }

        private async Task<byte[]> ReadResponseAsync(CancellationToken token)
        {
            // TODO change by PipeReader??
            using (var output = new MemoryStream(DefaultCapacity))
            {
                var buffer = new byte[DefaultCapacity];
                var bytes = await _stream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false);
                await output.WriteAsync(buffer, 0, bytes, token).ConfigureAwait(false);
                while (_client.Available != 0)
                {
                    bytes = await _stream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false);
                    await output.WriteAsync(buffer, 0, bytes, token).ConfigureAwait(false);
                }

                return output.ToArray();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_stream != null)
                {
                    _stream.Dispose();
                    _stream = null;
                }

                if (_client != null)
                {
                    _client.Dispose();
                    _client = null;
                }
            }
        }

        private static string Log(byte[] buffer)
        {
            return Encoding.UTF8.GetString(buffer);
        }
    }
}