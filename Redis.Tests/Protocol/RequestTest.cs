using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Framework.Caching.Redis.Protocol.Tests
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
        public void Write_WithNoArgs_WriteBuffer()
        {
            var expected = "DBSIZE\r\n";
            Memory<byte> memory = new byte[expected.Length];

            var target = new Request(CommandType.DBSize);
            var length = target.Write(memory);

            Assert.AreEqual(expected.Length, length);
            Assert.AreEqual(expected, Resp.Encoding.GetString(memory.ToArray()));
        }

        [TestMethod]
        public void Write_WithKey_WriteBuffer()
        {
            var expected = "GET foo\r\n";
            Memory<byte> memory = new byte[expected.Length];

            var target = new Request(CommandType.Get, "foo");
            var length = target.Write(memory);

            Assert.AreEqual(expected.Length, length);
            Assert.AreEqual(expected, Resp.Encoding.GetString(memory.ToArray()));
        }

        [TestMethod]
        public void Write_WithKeyAndValue_WriteBuffer()
        {
            var expected = "GETSET foo bar\r\n";
            Memory<byte> memory = new byte[expected.Length];

            var target = new Request(CommandType.GetSet, "foo", "bar");
            var length = target.Write(memory);

            Assert.AreEqual(expected.Length, length);
            Assert.AreEqual(expected, Resp.Encoding.GetString(memory.ToArray()));
        }

        [TestMethod]
        public void Write_WithKeyAndAbsoluteExpiration_WriteBuffer()
        {
            var expected = "PEXPIREAT foo 1501583410000\r\n";
            Memory<byte> memory = new byte[expected.Length];

            var target = new Request(CommandType.PExpireAt, "foo", new DateTimeOffset(2017, 8, 1, 10, 30, 10, TimeSpan.Zero));
            var length = target.Write(memory);

            Assert.AreEqual(expected.Length, length);
            Assert.AreEqual(expected, Resp.Encoding.GetString(memory.ToArray()));
        }

        [TestMethod]
        public void Write_WithKeyAndSlidingExpiration_WriteBuffer()
        {
            var expected = "PEXPIRE foo 18000000\r\n";
            Memory<byte> memory = new byte[expected.Length];

            var target = new Request(CommandType.PExpire, "foo", TimeSpan.FromHours(5));
            var length = target.Write(memory);

            Assert.AreEqual(expected.Length, length);
            Assert.AreEqual(expected, Resp.Encoding.GetString(memory.ToArray()));
        }
    }
}