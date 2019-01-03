using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Framework.Caching.Transport.Tests
{
    [TestClass]
    public class TcpTransportTest
    {
        [TestMethod]
        public void TcpTransport_ThrowsIfHostNull()
        {
            var e = Assert.ThrowsException<ArgumentNullException>(() => new TcpTransport(null, 10));
            Assert.AreEqual("host", e.ParamName);
        }

        [TestMethod]
        public void TcpTransport_ThrowsIfPortLessThanMinPort()
        {
            var e = Assert.ThrowsException<ArgumentOutOfRangeException>(() => new TcpTransport("foo", -1));
            Assert.AreEqual("port", e.ParamName);
        }

        [TestMethod]
        public void TcpTransport_ThrowsIfPortGreaterThanMaxPort()
        {
            var e = Assert.ThrowsException<ArgumentOutOfRangeException>(() => new TcpTransport("foo", 65536));
            Assert.AreEqual("port", e.ParamName);
        }

        [TestMethod]
        public void Send_ThrowsIfRequestNull()
        {
            var target = new TcpTransport("foo", 10);
            var e = Assert.ThrowsException<ArgumentNullException>(() => target.Send((byte[])null));

            Assert.AreEqual("request", e.ParamName);
        }

        [TestMethod]
        public async Task SendAsync_ThrowsIfRequestNull()
        {
            var target = new TcpTransport("foo", 10);
            var e = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => target.SendAsync((byte[])null));

            Assert.AreEqual("request", e.ParamName);
        }
    }
}