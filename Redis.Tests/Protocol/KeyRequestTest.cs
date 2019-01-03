using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;

namespace Framework.Caching.Protocol.Tests
{
    [TestClass]
    public class KeyRequestTest
    {
        [TestMethod]
        public void KeyRequest_ThrowsIfKeyNull()
        {
            var e = Assert.ThrowsException<ArgumentNullException>(() => new KeyRequest(CommandType.Get, null));

            Assert.AreEqual("key", e.ParamName);
        }

        [TestMethod]
        public void KeyRequest_ThrowsIfKeyEmpty()
        {
            var e = Assert.ThrowsException<ArgumentException>(() => new KeyRequest(CommandType.Get, string.Empty));

            Assert.AreEqual("key", e.ParamName);
        }

        [TestMethod]
        public void Write_GetDatagram()
        {
            var expected = "GET foo\r\n";
            var buffer = new byte[expected.Length];
            var memory = new Memory<byte>(buffer);

            var target = new KeyRequest(CommandType.Get, "foo");
            var size = target.Write(memory);

            Assert.AreEqual(expected.Length, size);
            Assert.AreEqual(expected, Encoding.UTF8.GetString(buffer));
        }
    }
}