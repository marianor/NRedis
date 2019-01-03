using System;
using System.Text;
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
            var e =Assert.ThrowsException<ArgumentNullException>(() => new KeysRequest(CommandType.MGet, keys));

            Assert.AreEqual("keys", e.ParamName);
        }

        [TestMethod]
        public void KeysRequest_ThrowsIfKeysEmpty()
        {
            var e =Assert.ThrowsException<ArgumentException>(() => new KeysRequest(CommandType.MGet));

            Assert.AreEqual("keys", e.ParamName);
        }

        [TestMethod]
        public void Buffer_GetDatagramUsingOneKey()
        {
            var target = new KeysRequest(CommandType.MGet, "foo");

            Assert.AreEqual("MGET foo\r\n", Encoding.UTF8.GetString(target.Buffer.Span));
        }

        [TestMethod]
        public void Buffer_GetDatagramUsingMultipleKeys()
        {
            var target = new KeysRequest(CommandType.MGet, "foo", "bar");

            Assert.AreEqual("MGET foo bar\r\n", Encoding.UTF8.GetString(target.Buffer.Span));
        }
    }
}