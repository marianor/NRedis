using Newtonsoft.Json;
using System.IO;
using System.IO.Compression;

namespace Framework.Caching.Redis
{
    public static class Serializer
    {
        private static readonly JsonSerializer m_serializer = new JsonSerializer();

        public static byte[] Serialize(object value)
        {
            if (value == null)
                return null;

            using (var stream = new MemoryStream())
            using (var zipStream = new DeflateStream(stream, CompressionMode.Compress))
            using (var textWriter = new StreamWriter(zipStream))
            using (var writer = new JsonTextWriter(textWriter))
            {
                m_serializer.Serialize(writer, value);
                writer.Close();
                return stream.ToArray();
            }
        }

        public static T Deserialize<T>(byte[] buffer)
        {
            if (buffer == null)
                return default;

            using (var stream = new MemoryStream(buffer))
            using (var zipStream = new DeflateStream(stream, CompressionMode.Decompress))
            using (var textReader = new StreamReader(zipStream))
            using (var reader = new JsonTextReader(textReader))
                return m_serializer.Deserialize<T>(reader);
        }
    }
}