namespace Framework.Caching.Redis.Protocol
{
    public enum CommandType
    {
        Auth,
        DBSize,
        Del,
        Exists,
        Get,
        GetSet,
        HGet,
        HMGet,
        HMSet,
        MGet,
        MSet,
        PExpire,
        PExpireAt,
        PTtl,
        Set
    }

    internal static class CommandTypeExtensions
    {
        public static string ToCommand(this CommandType commandType)
        {
            return commandType.ToString().ToUpperInvariant();
        }
    }
}