using Framework.Caching.Redis.Protocol;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Framework.Caching.Redis.Protocol.Tests
{
    [TestClass]
    public class KeyValueRequestTest
    {
        [TestMethod]
        public void KeyValueRequest_KeyIsNull_Throws()
        {
            var e = Assert.ThrowsException<ArgumentNullException>(() => new KeyValueRequest(CommandType.GetSet, null, "foo"));

            Assert.AreEqual("key", e.ParamName);
        }

        [TestMethod]
        public void KeyValueRequest_KeyIsEmpty_Throws()
        {
            var e = Assert.ThrowsException<ArgumentException>(() => new KeyValueRequest(CommandType.GetSet, string.Empty, "foo"));

            Assert.AreEqual("key", e.ParamName);
        }

        [TestMethod]
        public void Write_Valid_GetBuffer()
        {
            var expected = "GETSET foo bar\r\n";
            var buffer = new byte[expected.Length];
            var memory = new Memory<byte>(buffer);

            var target = new KeyValueRequest(CommandType.GetSet, "foo", "bar");
            var length = target.Write(memory);

            Assert.AreEqual(expected.Length, length);
            Assert.AreEqual(expected, RespProtocol.Encoding.GetString(buffer));
        }
    }
}