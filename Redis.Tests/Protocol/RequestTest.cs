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
        public void Write_GetDatagram()
        {
            var expected = "DBSIZE\r\n";
            var buffer = new byte[expected.Length];
            var memory = new Memory<byte>(buffer);

            var target = new Request(CommandType.DBSize);
            var size = target.Write(memory);

            Assert.AreEqual(expected.Length, size);
            Assert.AreEqual(expected, Encoding.UTF8.GetString(buffer));
        }
    }
}