using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Caching.Redis.Transport
{
    public class TcpTransport : ITransport, IDisposable
    {
        private TcpClient _client;
        private Stream _stream;

        public TcpTransport(string host, int port)
        {
            Host = host ?? throw new ArgumentNullException(nameof(host));
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

        public async Task ConnectAsync(CancellationToken token = default)
        {
            _client = new TcpClient();
            await _client.ConnectAsync(Host, Port).ConfigureAwait(false);
            _stream = await GetStreamAsync(_client).ConfigureAwait(false);
            State = TransportState.Connected;
        }

        protected virtual Stream GetStream(TcpClient client) => client.GetStream();

        protected virtual async Task<Stream> GetStreamAsync(TcpClient client) => await Task.FromResult(client.GetStream()).ConfigureAwait(false);

        public ReadOnlySequence<byte> Send(ReadOnlySequence<byte> request)
        {
            Logger?.LogTrace(() => $"[Request] {request.ToLogText()}");
            WriteRequest(request);

            var response = ReadResponse();
            Logger?.LogTrace(() => $"[Response] {response.ToLogText()}");
            return response;
        }

        public async Task<ReadOnlySequence<byte>> SendAsync(ReadOnlySequence<byte> request, CancellationToken token = default)
        {
            Logger?.LogTrace(() => $"[Request] {request.ToLogText()}");
            await WriteRequestAsync(request, token).ConfigureAwait(false);

            var response = await ReadResponseAsync(token).ConfigureAwait(false);
            Logger?.LogTrace(() => $"[Response] {response.ToLogText()}");
            return response;
        }

        private void WriteRequest(ReadOnlySequence<byte> request)
        {
            foreach (var memory in request)
            {
                var buffer = memory.AsSegment();
                _stream.Write(buffer.Array, buffer.Offset, buffer.Count);
            }

            _stream.Flush();
        }

        private async Task WriteRequestAsync(ReadOnlySequence<byte> request, CancellationToken token)
        {
            foreach (var memory in request)
            {
                var buffer = memory.AsSegment();
                await _stream.WriteAsync(buffer.Array, buffer.Offset, buffer.Count, token).ConfigureAwait(false);
            }

            await _stream.FlushAsync().ConfigureAwait(false);
        }

        private ReadOnlySequence<byte> ReadResponse()
        {
            var pipe = new Pipe();
            var writer = pipe.Writer;

            while (ReadChunk(writer)) ;
            writer.Complete();

            if (!pipe.Reader.TryRead(out ReadResult result))
                throw new InvalidOperationException(); // TODO exception

            pipe.Reader.Complete();
            return result.Buffer;
        }

        private async Task<ReadOnlySequence<byte>> ReadResponseAsync(CancellationToken token)
        {
            var pipe = new Pipe();
            var writer = pipe.Writer;

            while (await ReadChunkAsync(writer, token).ConfigureAwait(false)) ;
            writer.Complete();

            var result = await pipe.Reader.ReadAsync(token).ConfigureAwait(false);
            pipe.Reader.Complete();
            return result.Buffer;
        }

        private bool ReadChunk(PipeWriter writer)
        {
            var buffer = writer.GetMemory().AsSegment();
            var written = _stream.Read(buffer.Array, buffer.Offset, buffer.Count);
            writer.Advance(written);
            return buffer.Count == written;
        }

        private async Task<bool> ReadChunkAsync(PipeWriter writer, CancellationToken token)
        {
            var buffer = writer.GetMemory().AsSegment();
            var written = await _stream.ReadAsync(buffer.Array, buffer.Offset, buffer.Count, token).ConfigureAwait(false);
            writer.Advance(written);
            return buffer.Count == written;
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