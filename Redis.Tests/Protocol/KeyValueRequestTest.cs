using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;

namespace Framework.Caching.Protocol.Tests
{
    [TestClass]
    public class KeyValueRequestTest
    {
        [TestMethod]
        public void KeyValueRequest_ThrowsIfKeyNull()
        {
            var e = Assert.ThrowsException<ArgumentNullException>(() => new KeyValueRequest(CommandType.GetSet, null, "foo"));

            Assert.AreEqual("key", e.ParamName);
        }

        [TestMethod]
        public void KeyValueRequest_ThrowsIfKeyEmpty()
        {
            var e = Assert.ThrowsException<ArgumentException>(() => new KeyValueRequest(CommandType.GetSet, string.Empty, "foo"));

            Assert.AreEqual("key", e.ParamName);
        }

        [TestMethod]
        public void Write_GetDatagram()
        {
            var expected = "GETSET foo bar\r\n";
            var buffer = new byte[expected.Length];
            var memory = new Memory<byte>(buffer);

            var target = new KeyValueRequest(CommandType.GetSet, "foo", "bar");
            var size = target.Write(memory);

            Assert.AreEqual(expected.Length, size);
            Assert.AreEqual(expected, Encoding.UTF8.GetString(buffer));
        }
    }
}