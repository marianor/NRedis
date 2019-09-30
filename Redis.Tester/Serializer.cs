using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading.Tasks;

namespace NRedis
{
    public static class Serializer
    {
        public static async Task<byte[]> SerializeAsync(object value)
        {
            if (value == null)
                return null;

            using (var stream = new MemoryStream())
            using (var zipStream = new DeflateStream(stream, CompressionMode.Compress))
            {
                await JsonSerializer.SerializeAsync(zipStream, value).ConfigureAwait(false);
                return stream.ToArray();
            }
        }

        public static async ValueTask<T> DeserializeAsync<T>(byte[] buffer)
        {
            if (buffer == null)
                return default;

            using (var stream = new MemoryStream(buffer))
            using (var zipStream = new DeflateStream(stream, CompressionMode.Decompress))
                return await JsonSerializer.DeserializeAsync<T>(zipStream);
        }
    }
}