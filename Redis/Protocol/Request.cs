namespace Framework.Caching.Protocol
{
    public class Request : IRequest
    {
        public Request(RequestType requestType)
        {
            RequestType = requestType;
        }

        public string RequestText => RequestType.ToString().ToUpperInvariant() + RespClient.CR + RespClient.LF;

        public RequestType RequestType { get; private set; }
    }
}