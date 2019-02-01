﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace Framework.Caching.Redis.Protocol.Tests
{
    [TestClass]
    public class RespFormatterTester
    {
        [TestMethod]
        public void Format_RequestWithNoArgs_WriteBuffer()
        {
            var command = "DBSIZE";
            var expected = $"*1\r\n${command.Length}\r\n{command}\r\n\r\n";
            var request = Mock.Of<IRequest>(r => r.Command == command);
            var buffer = new byte[128];

            var length = request.Format(buffer);

            Assert.AreEqual(length, request.GetLength());
            Assert.AreEqual(expected, Resp.Encoding.GetString(buffer, 0, length));
        }

        [TestMethod]
        public void Format_RequestWithKey_WriteBuffer()
        {
            var command = "GET";
            var key = "foo";
            var expected = $"*2\r\n${command.Length}\r\n{command}\r\n${key.Length}\r\n{key}\r\n\r\n";
            var request = Mock.Of<IRequest>(r => r.Command == command && r.GetArgs() == new object[] { key });
            var buffer = new byte[128];

            var length = request.Format(buffer);

            Assert.AreEqual(length, request.GetLength());
            Assert.AreEqual(expected, Resp.Encoding.GetString(buffer, 0, length));
        }

        [TestMethod]
        public void Format_RequestWithKeyAndValue_WriteBuffer()
        {
            var command = "GETSET";
            var key = "foo";
            var value = "bar";
            var expected = $"*3\r\n${command.Length}\r\n{command}\r\n${key.Length}\r\n{key}\r\n${value.Length}\r\n{value}\r\n\r\n";
            var request = Mock.Of<IRequest>(r => r.Command == command && r.GetArgs() == new object[] { key, value });
            var buffer = new byte[128];

            var length = request.Format(buffer);

            Assert.AreEqual(length, request.GetLength());
            Assert.AreEqual(expected, Resp.Encoding.GetString(buffer, 0, length));
        }

        [TestMethod]
        public void Format_RequestWithKeyAndAbsoluteExpiration_WriteBuffer()
        {
            var command = "PEXPIREAT";
            var key = "foo";
            var time = new DateTimeOffset(2017, 8, 1, 10, 30, 10, TimeSpan.Zero);
            var expected = $"*3\r\n${command.Length}\r\n{command}\r\n${key.Length}\r\n{key}\r\n:{time.ToUnixTimeMilliseconds()}\r\n\r\n";
            var request = Mock.Of<IRequest>(r => r.Command == command && r.GetArgs() == new object[] { key, time });
            var buffer = new byte[128];

            var length = request.Format(buffer);

            Assert.AreEqual(length, request.GetLength());
            Assert.AreEqual(expected, Resp.Encoding.GetString(buffer, 0, length));
        }

        [TestMethod]
        public void Format_RequestWithKeyAndSlidingExpiration_WriteBuffer()
        {
            var command = "PEXPIRE";
            var key = "foo";
            var timeSpan = TimeSpan.FromMilliseconds(18000000);
            var expected = $"*3\r\n${command.Length}\r\n{command}\r\n${key.Length}\r\n{key}\r\n:{timeSpan.TotalMilliseconds:0}\r\n\r\n";
            var request = Mock.Of<IRequest>(r => r.Command == command && r.GetArgs() == new object[] { key, timeSpan });
            var buffer = new byte[128];

            var length = request.Format(buffer);

            Assert.AreEqual(length, request.GetLength());
            Assert.AreEqual(expected, Resp.Encoding.GetString(buffer, 0, length));
        }
    }
}