namespace Framework.Caching.Transport
{
    public interface ITransportSettings
    {
        string Host { get; set; }

        string Password { get; set; }

        int Port { get; set; }

        ITransport CreateTransport();
    }
}