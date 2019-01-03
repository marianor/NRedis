using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;

namespace Framework.Caching.Protocol.Tests
{
    [TestClass]
    public class RequestTest
    {
        [TestMethod]
        public void Request_ThrowsIfCommandNull()
        {
            var e = Assert.ThrowsException<ArgumentNullException>(() => new Request(null));

            Assert.AreEqual("command", e.ParamName);
        }

        [TestMethod]
        public void Request_ThrowsIfCommandEmpty()
        {
            var e = Assert.ThrowsException<ArgumentException>(() => new Request(string.Empty));

            Assert.AreEqual("command", e.ParamName);
        }

        [TestMethod]
        public void Buffer_GetDatagram()
        {
            var target = new Request(CommandType.DBSize);

            Assert.AreEqual("DBSIZE\r\n", Encoding.UTF8.GetString(target.Buffer.Span));
        }
    }
}