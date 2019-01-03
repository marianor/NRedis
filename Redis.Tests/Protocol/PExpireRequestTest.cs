using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Framework.Caching.Protocol.Tests
{
    [TestClass]
    public class PExpireRequestTest
    {
        [TestMethod]
        public void PExpireRequest_ThrowsIfKeyNull()
        {
            var e =Assert.ThrowsException<ArgumentNullException>(() => new PExpireRequest(null, TimeSpan.FromHours(5)));

            Assert.AreEqual("key", e.ParamName);
        }

        [TestMethod]
        public void PExpireAtRequest_ThrowsIfKeyEmpty()
        {
            var e =Assert.ThrowsException<ArgumentException>(() => new PExpireRequest(string.Empty, TimeSpan.FromHours(5)));

            Assert.AreEqual("key", e.ParamName);
        }

        [TestMethod]
        public void PExpireAtRequest_ThrowsIfSlidingExpirationZero()
        {
            var e =Assert.ThrowsException<ArgumentOutOfRangeException>(() => new PExpireRequest("foo", TimeSpan.Zero));

            Assert.AreEqual("slidingExpiration", e.ParamName);
        }

        [TestMethod]
        public void PExpireAtRequest_ThrowsIfSlidingExpirationNegative()
        {
            var e =Assert.ThrowsException<ArgumentOutOfRangeException>(() => new PExpireRequest("foo", TimeSpan.FromMinutes(-1)));

            Assert.AreEqual("slidingExpiration", e.ParamName);
        }

        [TestMethod]
        public void Buffer_GetDatagram()
        {
            var target = new PExpireRequest("foo", TimeSpan.FromHours(5));

            Assert.AreEqual("PEXPIRE foo 18000000\r\n", Encoding.UTF8.GetString(target.Buffer.Span));
        }
    }
}