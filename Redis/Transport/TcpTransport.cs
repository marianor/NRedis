using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Caching.Transport
{
    public class TcpTransport : ITransport, IDisposable
    {
        private const int DefaultCapacity = 4096;

        private readonly Encoding _encoding = Encoding.UTF8;
        private TcpClient _client;
        private Stream _stream;

        public TcpTransport(string host, int port)
        {
            Host = host;
            Port = port;
        }

        public string Host { get; }

        public int Port { get; }

        public ILogger Logger { get; set; }

        public TransportState State { get; private set; }

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
            await _client.ConnectAsync(Host, Port);
            _stream = await GetStreamAsync(_client);
            State = TransportState.Connected;
        }

        protected virtual Stream GetStream(TcpClient client) => client.GetStream();

        protected virtual async Task<Stream> GetStreamAsync(TcpClient client) => await Task.FromResult<Stream>(client.GetStream());

        // TODO rename as ????
        public string Send(string request)
        {
            var x = SendAsync(request).ConfigureAwait(true).GetAwaiter();

            Logger?.LogInformation($"[Request]\r\n{request}");

            var buffer = _encoding.GetBytes(request);
            _stream.Write(buffer, 0, buffer.Length);

            using (var output = new MemoryStream(DefaultCapacity))
            {
                var outputBuffer = new byte[DefaultCapacity];
                var bytes = _stream.Read(outputBuffer, 0, outputBuffer.Length);
                output.Write(outputBuffer, 0, bytes);
                while (_client.Available != 0)
                {
                    bytes = _stream.Read(outputBuffer, 0, outputBuffer.Length);
                    output.Write(outputBuffer, 0, bytes);
                }

                var response = _encoding.GetString(output.ToArray());
                Logger?.LogInformation($"[Response]\r\n{response}");
                return response;
            }
        }

        public async Task<string> SendAsync(string request, CancellationToken token = default(CancellationToken))
        {
            Logger?.LogInformation($"[Request]\r\n{request}");
            await WriteRequestAsync(request, token);

            var response = await ReadResponseAsync(token);
            Logger?.LogInformation($"[Response]\r\n{response}");
            return response;
        }

        private async Task WriteRequestAsync(string request, CancellationToken token)
        {
            var buffer = _encoding.GetBytes(request);
            await _stream.WriteAsync(buffer, 0, buffer.Length, token);
        }

        private async Task<string> ReadResponseAsync(CancellationToken token)
        {
            using (var output = new MemoryStream(DefaultCapacity))
            {
                var buffer = new byte[DefaultCapacity];
                var bytes = await _stream.ReadAsync(buffer, 0, buffer.Length, token);
                await output.WriteAsync(buffer, 0, bytes, token);
                while (_client.Available != 0)
                {
                    bytes = await _stream.ReadAsync(buffer, 0, buffer.Length, token);
                    await output.WriteAsync(buffer, 0, bytes, token);
                }

                return _encoding.GetString(output.ToArray());
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
    }
}