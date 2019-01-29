using System;
using System.Collections.Generic;

namespace Framework.Caching.Redis.Protocol
{
    internal class RespFormatter
    {
        public void Format(IRequest request, byte[] buffer)
        {
            // TODO replace by PIPE.IO ?
            Memory<byte> memory = buffer;
            request.Write(memory);
        }

        public int Format(IEnumerable<IRequest> requests, byte[] buffer)
        {
            // TODO replace by PIPE.IO ?
            var count = 0;
            Memory<byte> memory = buffer;
            foreach (var request in requests)
            {
                request.Write(memory);
                count++;
            }

            return count;
        }
    }
}