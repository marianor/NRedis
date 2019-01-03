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
        public void Buffer_GetDatagram()
        {
            var target = new KeyValueRequest(CommandType.GetSet, "foo", "bar");

            Assert.AreEqual("GETSET foo bar\r\n", Encoding.UTF8.GetString(target.Buffer.Span));
        }
    }
}