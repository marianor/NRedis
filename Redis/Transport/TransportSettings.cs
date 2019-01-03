using System;
using Microsoft.Extensions.Logging;

namespace Framework.Caching.Transport
{
    public class TransportSettings<T> : ITransportSettings where T : ITransport
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public string Password { get; set; }

        // TODO should it be here ???
        public ILogger Logger { get; set; }

        public ITransport CreateTransport()
        {
            // TODO should be by interface
            var transport = (TcpTransport)Activator.CreateInstance(typeof(T), Host, Port);
            transport.Logger = Logger;
            return transport;
        }
    }
}