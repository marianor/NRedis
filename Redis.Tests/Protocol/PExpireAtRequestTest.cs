using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Framework.Caching.Protocol.Tests
{
    [TestClass]
    public class PExpireAtRequestTest
    {
        [TestMethod]
        public void PExpireAtRequest_ThrowsIfKeyNull()
        {
            var e =Assert.ThrowsException<ArgumentNullException>(() => new PExpireAtRequest(null, DateTimeOffset.Now));

            Assert.AreEqual("key", e.ParamName);
        }

        [TestMethod]
        public void PExpireAtRequest_ThrowsIfKeyEmpty()
        {
            var e =Assert.ThrowsException<ArgumentException>(() => new PExpireAtRequest(string.Empty, DateTimeOffset.Now));

            Assert.AreEqual("key", e.ParamName);
        }

        [TestMethod]
        public void Buffer_GetDatagram()
        {
            var target = new PExpireAtRequest("foo", new DateTimeOffset(2017, 8, 1, 10, 30, 10, TimeSpan.Zero));

            Assert.AreEqual("PEXPIREAT foo 1501583410000\r\n", Encoding.UTF8.GetString(target.Buffer.Span));
        }
    }
}