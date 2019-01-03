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
        public void Buffer_GetDatagram()
        {
            var target = new KeyRequest(CommandType.Get, "foo");

            Assert.AreEqual("GET foo\r\n", Encoding.UTF8.GetString(target.Buffer.Span));
        }
    }
}