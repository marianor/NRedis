using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;

namespace Framework.Caching.Protocol.Tests
{
    [TestClass]
    public class PExpireAtRequestTest
    {
        [TestMethod]
        public void PExpireAtRequest_ThrowsIfKeyNull()
        {
            var e = Assert.ThrowsException<ArgumentNullException>(() => new PExpireAtRequest(null, DateTimeOffset.Now));

            Assert.AreEqual("key", e.ParamName);
        }

        [TestMethod]
        public void PExpireAtRequest_ThrowsIfKeyEmpty()
        {
            var e = Assert.ThrowsException<ArgumentException>(() => new PExpireAtRequest(string.Empty, DateTimeOffset.Now));

            Assert.AreEqual("key", e.ParamName);
        }

        [TestMethod]
        public void Write_GetDatagram()
        {
            var expected = "PEXPIREAT foo 1501583410000\r\n";
            var buffer = new byte[expected.Length];
            var memory = new Memory<byte>(buffer);

            var target = new PExpireAtRequest("foo", new DateTimeOffset(2017, 8, 1, 10, 30, 10, TimeSpan.Zero));
            var size = target.Write(memory);

            Assert.AreEqual(expected.Length, size);
            Assert.AreEqual(expected, Encoding.UTF8.GetString(buffer));
        }
    }
}