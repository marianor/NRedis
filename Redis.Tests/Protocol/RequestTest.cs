using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;

namespace Framework.Caching.Protocol.Tests
{
    [TestClass]
    public class RequestTest
    {
        [TestMethod]
        public void Request_CommandIsNull_Throws()
        {
            var e = Assert.ThrowsException<ArgumentNullException>(() => new Request(null));

            Assert.AreEqual("command", e.ParamName);
        }

        [TestMethod]
        public void Request_CommandIsEmpty_Throws()
        {
            var e = Assert.ThrowsException<ArgumentException>(() => new Request(string.Empty));

            Assert.AreEqual("command", e.ParamName);
        }

        [TestMethod]
        public void Write_Valid_WriteBuffer()
        {
            var expected = "DBSIZE\r\n";
            var buffer = new byte[expected.Length];
            var memory = new Memory<byte>(buffer);

            var target = new Request(CommandType.DBSize);
            var length = target.Write(memory);

            Assert.AreEqual(expected.Length, length);
            Assert.AreEqual(expected, Encoding.UTF8.GetString(buffer));
        }
    }
}