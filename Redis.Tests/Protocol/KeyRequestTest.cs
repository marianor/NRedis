using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Framework.Caching.Protocol.Tests
{
    [TestClass]
    public class KeyRequestTest
    {
        [TestMethod]
        public void KeyRequest_ThrowsIfKeyNull()
        {
            var e = Assert.ThrowsException<ArgumentNullException>(() => new KeyRequest(RequestType.Get, null));

            Assert.AreEqual("key", e.ParamName);
        }

        [TestMethod]
        public void KeyRequest_ThrowsIfKeyEmpty()
        {
            var e =Assert.ThrowsException<ArgumentException>(() => new KeyRequest(RequestType.Get, string.Empty));

            Assert.AreEqual("key", e.ParamName);
        }

        [TestMethod]
        public void RequestText_GetDatagram()
        {
            var target = new KeyRequest(RequestType.Get, "foo");

            Assert.AreEqual("GET foo\r\n", target.RequestText);
        }
    }
}