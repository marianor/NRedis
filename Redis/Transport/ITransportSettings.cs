namespace Framework.Caching.Transport
{
    public interface ITransportSettings
    {
        string Host { get; set; }

        string Password { get; set; } // TODO use a SecureString instead

        int Port { get; set; }

        ITransport CreateTransport();
    }
}