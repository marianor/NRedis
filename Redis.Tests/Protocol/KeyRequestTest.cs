using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;

namespace Framework.Caching.Protocol.Tests
{
    [TestClass]
    public class KeyRequestTest
    {
        [TestMethod]
        public void KeyRequest_KeyIsNull_Throws()
        {
            var e = Assert.ThrowsException<ArgumentNullException>(() => new KeyRequest(CommandType.Get, null));

            Assert.AreEqual("key", e.ParamName);
        }

        [TestMethod]
        public void KeyRequest_KeyIsEmpty_Throws()
        {
            var e = Assert.ThrowsException<ArgumentException>(() => new KeyRequest(CommandType.Get, string.Empty));

            Assert.AreEqual("key", e.ParamName);
        }

        [TestMethod]
        public void Write_Valid_WriteBuffer()
        {
            var expected = "GET foo\r\n";
            var buffer = new byte[expected.Length];
            var memory = new Memory<byte>(buffer);

            var target = new KeyRequest(CommandType.Get, "foo");
            var length = target.Write(memory);

            Assert.AreEqual(expected.Length, length);
            Assert.AreEqual(expected, Protocol.Encoding.GetString(buffer));
        }
    }
}