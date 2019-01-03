using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;

namespace Framework.Caching.Protocol.Tests
{
    [TestClass]
    public class KeysRequestTest
    {
        [TestMethod]
        public void KeysRequest_ThrowsIfKeysNull()
        {
            string[] keys = null;
            var e = Assert.ThrowsException<ArgumentNullException>(() => new KeysRequest(CommandType.MGet, keys));

            Assert.AreEqual("keys", e.ParamName);
        }

        [TestMethod]
        public void KeysRequest_ThrowsIfKeysEmpty()
        {
            var e = Assert.ThrowsException<ArgumentException>(() => new KeysRequest(CommandType.MGet));

            Assert.AreEqual("keys", e.ParamName);
        }

        [TestMethod]
        public void Write_GetDatagramUsingOneKey()
        {
            var expected = "MGET foo\r\n";
            var buffer = new byte[expected.Length];
            var memory = new Memory<byte>(buffer);

            var target = new KeysRequest(CommandType.MGet, "foo");
            var size = target.Write(memory);

            Assert.AreEqual(expected.Length, size);
            Assert.AreEqual(expected, Encoding.UTF8.GetString(buffer));
        }

        [TestMethod]
        public void Write_GetDatagramUsingMultipleKeys()
        {
            var expected = "MGET foo bar\r\n";
            var buffer = new byte[expected.Length];
            var memory = new Memory<byte>(buffer);

            var target = new KeysRequest(CommandType.MGet, "foo", "bar");
            var size = target.Write(memory);

            Assert.AreEqual(expected.Length, size);
            Assert.AreEqual(expected, Encoding.UTF8.GetString(buffer));
        }
    }
}