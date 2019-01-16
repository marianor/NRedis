using System;
using System.Diagnostics;
using System.IO;

namespace Framework.Caching.Redis.Tester
{
    public sealed class LocalRedisLauncher : IDisposable
    {
        private readonly Process _redisServer;

        public LocalRedisLauncher(string redisDirectory)
        {
            if (redisDirectory == null)
                throw new ArgumentNullException(nameof(redisDirectory));

            _redisServer = new Process
            {
                StartInfo = new ProcessStartInfo(Path.Combine(redisDirectory, @"redis-server.exe"))
                {
                    WorkingDirectory = redisDirectory,
                    Arguments = @"redis.windows.conf",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            if (!_redisServer.Start())
                throw new InvalidOperationException("Cannot start server");
        }


        public void Dispose()
        {
            if (!_redisServer.HasExited)
                _redisServer.Kill();
            _redisServer.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}