namespace Framework.Caching.Protocol
{
    public enum CommandType
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

    public static class CommandTypeExtensions
    {
        public static string ToCommand(this CommandType commandType)
        {
            return commandType.ToString().ToUpperInvariant();
        }
    }
}