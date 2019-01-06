using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Framework
{
    internal static class StringExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Format(this string format, params object[] args)
        {
            return string.Format(CultureInfo.CurrentCulture, format, args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogTrace(this ILogger logger, Func<string> formatter)
        {
            logger.Log(LogLevel.Trace, 0, (object)null, null, (s, e) => formatter());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToLogText(this byte[] buffer)
        {
            return Encoding.UTF8.GetString(buffer).Replace("\r", "\\r").Replace("\n", "\\n");
        }
    }
}