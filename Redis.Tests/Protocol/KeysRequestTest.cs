using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Framework.Caching.Protocol.Tests
{
    [TestClass]
    public class KeysRequestTest
    {
        [TestMethod]
        public void KeysRequest_ThrowsIfKeysNull()
        {
            string[] keys = null;
            var e =Assert.ThrowsException<ArgumentNullException>(() => new KeysRequest(RequestType.MGet, keys));

            Assert.AreEqual("keys", e.ParamName);
        }

        [TestMethod]
        public void KeysRequest_ThrowsIfKeysEmpty()
        {
            var e =Assert.ThrowsException<ArgumentException>(() => new KeysRequest(RequestType.MGet));

            Assert.AreEqual("keys", e.ParamName);
        }

        [TestMethod]
        public void RequestText_GetDatagramUsingOneKey()
        {
            var target = new KeysRequest(RequestType.MGet, "foo");

            Assert.AreEqual("MGET foo\r\n", target.RequestText);
        }

        [TestMethod]
        public void RequestText_GetDatagramUsingMultipleKeys()
        {
            var target = new KeysRequest(RequestType.MGet, "foo", "bar");

            Assert.AreEqual("MGET foo bar\r\n", target.RequestText);
        }
    }
}