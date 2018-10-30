namespace Framework.Caching.Protocol
{
    public interface IRequest
    {
        RequestType RequestType { get; }

        string RequestText { get; }
    }
}