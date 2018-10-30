namespace Framework.Caching.Protocol
{
    public enum RequestType
    {
        Auth,
        DBSize,
        Del,
        Exists,
        Get,
        GetSet,
        MGet,
        MSet,
        PExpire,
        PExpireAt,
        Set
    }
}