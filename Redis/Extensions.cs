using Microsoft.Extensions.Logging;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Framework.Caching.Redis
{
    internal static class Extensions
    {
        private static readonly IReadOnlyDictionary<byte, string> m_replace = new Dictionary<byte, string>() { { (byte)'\r', "\\r" }, { (byte)'\n', "\\n" }, { (byte)'\t', "\\t" } };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] AsBuffer(this TimeSpan timeSpan)
        {
            var milliseconds = (long)timeSpan.TotalMilliseconds;
            Span<byte> buffer = new byte[8];
            if (Utf8Formatter.TryFormat(milliseconds, buffer, out int written))
                return buffer.Slice(0, written).ToArray();

            throw new InvalidOperationException(); // TODO make message
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Format(this string format, params object[] args) => string.Format(CultureInfo.CurrentCulture, format, args);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogTrace(this ILogger logger, Func<string> formatter) => logger.Log(LogLevel.Trace, 0, (object)null, null, (s, e) => formatter());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToLogText(this byte[] buffer, int offset, int count)
        {
            var builder = new StringBuilder(buffer.Length * 15 / 10);
            foreach (var c in new ReadOnlySpan<byte>(buffer, offset, count))
                if (m_replace.TryGetValue(c, out string replace))
                    builder.Append(replace);
                else if (char.IsControl((char)c))
                    builder.AppendFormat(CultureInfo.InvariantCulture, "\\x{0:X}", c);
                else
                    builder.Append((char)c);

            return builder.ToString();
        }
    }
}