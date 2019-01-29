using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Framework.Caching.Redis.Transport.Tests
{
    [TestClass]
    public class TcpTransportTest
    {
        [TestMethod]
        public void TcpTransport_HostIsNull_Throws()
        {
            var e = Assert.ThrowsException<ArgumentNullException>(() => new TcpTransport(null, 0));

            Assert.AreEqual("host", e.ParamName);
        }

        [TestMethod]
        public void TcpTransport_PortIsLessThanMinPort_Throws()
        {
            var e = Assert.ThrowsException<ArgumentOutOfRangeException>(() => new TcpTransport("foo", -1));

            Assert.AreEqual("port", e.ParamName);
        }

        [TestMethod]
        public void TcpTransport_PortIsGreaterThanMaxPort_Throws()
        {
            var e = Assert.ThrowsException<ArgumentOutOfRangeException>(() => new TcpTransport("foo", 65536));

            Assert.AreEqual("port", e.ParamName);
        }

        [TestMethod]
        public void Send_RequestIsNull_Throws()
        {
            var target = new TcpTransport("foo", 0);
            var e = Assert.ThrowsException<ArgumentNullException>(() => target.Send(null, 0, 1));

            Assert.AreEqual("request", e.ParamName);
        }

        [TestMethod]
        public async Task SendAsync_RequestIsNull_Throws()
        {
            var target = new TcpTransport("foo", 0);
            var e = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => target.SendAsync(null, 0, 1));

            Assert.AreEqual("request", e.ParamName);
        }
    }
}