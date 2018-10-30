using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Framework.Caching.Protocol.Tests
{
    [TestClass]
    public class KeyValueRequestTest
    {
        [TestMethod]
        public void KeyValueRequest_ThrowsIfKeyNull()
        {
            var e =Assert.ThrowsException<ArgumentNullException>(() => new KeyValueRequest(RequestType.GetSet, null, "foo"));

            Assert.AreEqual("key", e.ParamName);
        }

        [TestMethod]
        public void KeyValueRequest_ThrowsIfKeyEmpty()
        {
            var e =Assert.ThrowsException<ArgumentException>(() => new KeyValueRequest(RequestType.GetSet, string.Empty, "foo"));

            Assert.AreEqual("key", e.ParamName);
        }

        [TestMethod]
        public void RequestText_GetDatagram()
        {
            var target = new KeyValueRequest(RequestType.GetSet, "foo", "bar");

            Assert.AreEqual("GETSET foo bar\r\n", target.RequestText);
        }
    }
}