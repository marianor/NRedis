using Newtonsoft.Json;
using System;
using System.Buffers.Text;
using System.IO;
using System.IO.Compression;
using System.Text;

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
                //Base64.EncodeToUtf8InPlace
                return Encoding.UTF8.GetBytes(Convert.ToBase64String(stream.ToArray()));
            }
        }

        public static T Deserialize<T>(byte[] buffer)
        {
            if (buffer == null)
                return default;

            Base64.DecodeFromUtf8InPlace(buffer, out int bytesWritten);
            var encoded = buffer; // Convert.FromBase64String(Encoding.UTF8.GetString(buffer));
            using (var stream = new MemoryStream(encoded))
            using (var zipStream = new DeflateStream(stream, CompressionMode.Decompress))
            using (var textReader = new StreamReader(zipStream))
            using (var reader = new JsonTextReader(textReader))
            {
                return m_serializer.Deserialize<T>(reader);
            }
        }
    }
}