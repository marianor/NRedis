using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace NRedis
{
    internal static class Extensions
    {
        private static readonly IReadOnlyDictionary<byte, string> m_replace = new Dictionary<byte, string>() { { (byte)'\r', "\\r" }, { (byte)'\n', "\\n" }, { (byte)'\t', "\\t" } };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Format(this string format, params object[] args) => string.Format(CultureInfo.CurrentCulture, format, args);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogTrace(this ILogger logger, Func<string> formatter) => logger.Log(LogLevel.Trace, 0, (object)null, null, (s, e) => formatter());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToLogText(this ReadOnlySequence<byte> buffer)
        {
            var builder = new StringBuilder();
            foreach (var memory in buffer)
                foreach (var b in memory.Span)
                    if (m_replace.TryGetValue(b, out var replace))
                        builder.Append(replace);
                    else if (char.IsControl((char)b))
                        builder.AppendFormat(CultureInfo.InvariantCulture, "\\x{0:X}", b);
                    else
                        builder.Append((char)b);

            return builder.ToString();
        }
    }
}
