using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Framework.Caching.Redis.Protocol.Tests
{
    [TestClass]
    public class PExpireAtRequestTest
    {
        [TestMethod]
        public void PExpireAtRequest_KeyIsNull_Throws()
        {
            var e = Assert.ThrowsException<ArgumentNullException>(() => new PExpireAtRequest(null, DateTimeOffset.Now));

            Assert.AreEqual("key", e.ParamName);
        }

        [TestMethod]
        public void PExpireAtRequest_KeyIsEmpty_Throws()
        {
            var e = Assert.ThrowsException<ArgumentException>(() => new PExpireAtRequest(string.Empty, DateTimeOffset.Now));

            Assert.AreEqual("key", e.ParamName);
        }

        [TestMethod]
        public void Write_Valid_WriteBuffer()
        {
            var expected = "PEXPIREAT foo 1501583410000\r\n";
            var buffer = new byte[expected.Length];
            var memory = new Memory<byte>(buffer);

            var target = new PExpireAtRequest("foo", new DateTimeOffset(2017, 8, 1, 10, 30, 10, TimeSpan.Zero));
            var length = target.Write(memory);

            Assert.AreEqual(expected.Length, length);
            Assert.AreEqual(expected, RespProtocol.Encoding.GetString(buffer));
        }
    }
}