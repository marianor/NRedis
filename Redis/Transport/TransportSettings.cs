using System;

namespace Framework.Caching.Transport
{
    public class TransportSettings<T> : ITransportSettings where T : ITransport
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public string Password { get; set; }

        public ITransport CreateTransport() => (ITransport)Activator.CreateInstance(typeof(T), Host, Port);
    }
}