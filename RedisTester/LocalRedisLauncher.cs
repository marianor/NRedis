using System;
using System.Diagnostics;
using System.IO;

namespace RedisTester
{
    public class LocalRedisLauncher : IDisposable
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
                    UseShellExecute = false
                }
            };

            if (!_redisServer.Start())
                throw new InvalidOperationException("Cannot start server");
        }


        public void Dispose()
        {
            _redisServer.Kill();
        }
    }
}