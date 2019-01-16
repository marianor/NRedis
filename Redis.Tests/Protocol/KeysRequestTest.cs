using Framework.Caching.Redis.Protocol;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;

namespace Framework.Caching.Redis.Protocol.Tests
{
    [TestClass]
    public class KeysRequestTest
    {
        [TestMethod]
        public void KeysRequest_KeysIsNull_Throws()
        {
            string[] keys = null;
            var e = Assert.ThrowsException<ArgumentNullException>(() => new KeysRequest(CommandType.MGet, keys));

            Assert.AreEqual("keys", e.ParamName);
        }

        [TestMethod]
        public void KeysRequest_KeysIsEmpty_Throws()
        {
            var e = Assert.ThrowsException<ArgumentException>(() => new KeysRequest(CommandType.MGet));

            Assert.AreEqual("keys", e.ParamName);
        }

        [TestMethod]
        public void Write_UsingOneKey_GetBuffer()
        {
            var expected = "MGET foo\r\n";
            var buffer = new byte[expected.Length];
            var memory = new Memory<byte>(buffer);

            var target = new KeysRequest(CommandType.MGet, "foo");
            var size = target.Write(memory);

            Assert.AreEqual(expected.Length, size);
            Assert.AreEqual(expected, RespProtocol.Encoding.GetString(buffer));
        }

        [TestMethod]
        public void Write_UsingMultipleKeys_GetBuffer()
        {
            var expected = "MGET foo bar\r\n";
            var buffer = new byte[expected.Length];
            var memory = new Memory<byte>(buffer);

            var target = new KeysRequest(CommandType.MGet, "foo", "bar");
            var length = target.Write(memory);

            Assert.AreEqual(expected.Length, length);
            Assert.AreEqual(expected, RespProtocol.Encoding.GetString(buffer));
        }
    }
}