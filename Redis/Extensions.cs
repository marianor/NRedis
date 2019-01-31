using Framework.Caching.Redis.Protocol;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Framework.Caching.Redis
{
    internal static class StringExtensions
    {
        private static readonly IReadOnlyDictionary<char, string> m_replace = new Dictionary<char, string>() { { '\r', "\\r" }, { '\n', "\\n" }, { '\t', "\\t" } };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Format(this string format, params object[] args) => string.Format(CultureInfo.CurrentCulture, format, args);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogTrace(this ILogger logger, Func<string> formatter) => logger.Log(LogLevel.Trace, 0, (object)null, null, (s, e) => formatter());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToLogText(this byte[] buffer)
        {
            var builder = new StringBuilder(buffer.Length * 15 / 10);
            foreach (var c in Resp.Encoding.GetString(buffer))
                if (m_replace.TryGetValue(c, out string replace))
                    builder.Append(replace);
                else
                    builder.Append(c);

            return builder.ToString();
        }
    }
}