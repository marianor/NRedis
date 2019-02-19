using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace NRedis.Transport.Tests
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
    }
}