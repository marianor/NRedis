using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
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
            _stream.Write(request);

            var response = ReadResponse();
            Logger?.LogTrace(() => $"[Response] {response.ToLogText()}");
            return response;
        }

        public async Task<ReadOnlySequence<byte>> SendAsync(ReadOnlySequence<byte> request, CancellationToken token = default)
        {
            Logger?.LogTrace(() => $"[Request] {request.ToLogText()}");
            await _stream.WriteAsync(request, token).ConfigureAwait(false);

            var response = await ReadResponseAsync(token).ConfigureAwait(false);
            Logger?.LogTrace(() => $"[Response] {response.ToLogText()}");
            return response;
        }

        private ReadOnlySequence<byte> ReadResponse()
        {
            var pipe = new Pipe();
            var writer = pipe.Writer;

            bool hasMoreData;
            do
            {
                hasMoreData = ReadChunk(writer);
            }
            while (hasMoreData);

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

            bool hasMoreData;
            do
            {
                hasMoreData = await ReadChunkAsync(writer, token).ConfigureAwait(false);
            }
            while (hasMoreData);

            writer.Complete();

            var result = await pipe.Reader.ReadAsync(token).ConfigureAwait(false);
            pipe.Reader.Complete();
            return result.Buffer;
        }

        private bool ReadChunk(PipeWriter writer)
        {
            var memory = writer.GetMemory();
            if (!MemoryMarshal.TryGetArray(memory, out ArraySegment<byte> buffer))
                throw new InvalidOperationException(); // TDOO ecceptions

            var written = _stream.Read(buffer.Array, 0, memory.Length);
            writer.Advance(written);
            return memory.Length == written;
        }

        private async Task<bool> ReadChunkAsync(PipeWriter writer, CancellationToken token)
        {
            var memory = writer.GetMemory();
            if (!MemoryMarshal.TryGetArray(memory, out ArraySegment<byte> buffer))
                throw new InvalidOperationException(); // TDOO ecceptions

            var written = await _stream.ReadAsync(buffer.Array, 0, memory.Length, token).ConfigureAwait(false);
            writer.Advance(written);
            return memory.Length == written;
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

    [Obsolete("Move to other file")]
    internal static class X
    {
        public static void Write(this Stream stream, ReadOnlySequence<byte> buffer)
        {
            foreach (var y in buffer)
                stream.Write(y.Span.ToArray(), 0, y.Span.Length);
            stream.Flush();
        }

        public static async Task WriteAsync(this Stream stream, ReadOnlySequence<byte> buffer, CancellationToken token)
        {
            foreach (var y in buffer)
                await stream.WriteAsync(y.Span.ToArray(), 0, y.Span.Length, token).ConfigureAwait(false);
            await stream.FlushAsync(token).ConfigureAwait(false);
        }
    }
}