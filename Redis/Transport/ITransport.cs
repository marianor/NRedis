﻿using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Caching.Redis.Transport
{
    public interface ITransport
    {
        ILogger Logger { get; set; }

        TransportState State { get; }

        void Connect();

        Task ConnectAsync(CancellationToken token);

        byte[] Send(byte[] request);

        Task<byte[]> SendAsync(byte[] request, CancellationToken token);
    }
}