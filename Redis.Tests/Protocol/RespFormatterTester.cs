using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Caching.Redis.Protocol.Tests
{
    [TestClass]
    public class RespFormatterTester
    {
        [TestMethod]
        public void Format_RequestWithNoArgs_WriteBuffer()
        {
            var command = "DBSIZE";
            var expected = Resp.Encoding.GetBytes($"*1\r\n${command.Length}\r\n{command}\r\n\r\n");
            var request = Mock.Of<IRequest>(r => r.Command == command);

            var buffer = request.Format();

            CollectionAssert.AreEqual(expected, buffer.ToArray());
        }

        [TestMethod]
        public void Format_RequestWithKey_WriteBuffer()
        {
            var command = "GET";
            var key = "foo";
            var expected = Resp.Encoding.GetBytes($"*2\r\n${command.Length}\r\n{command}\r\n${key.Length}\r\n{key}\r\n\r\n");
            var request = Mock.Of<IRequest>(r => r.Command == command && r.GetArgs() == new object[] { key });

            var buffer = request.Format();

            CollectionAssert.AreEqual(expected, buffer.ToArray());
        }

        [TestMethod]
        public void Format_RequestWithKeyAndValue_WriteBuffer()
        {
            var command = "GETSET";
            var key = "foo";
            var value = "bar";
            var expected = Resp.Encoding.GetBytes($"*3\r\n${command.Length}\r\n{command}\r\n${key.Length}\r\n{key}\r\n${value.Length}\r\n{value}\r\n\r\n");
            var request = Mock.Of<IRequest>(r => r.Command == command && r.GetArgs() == new object[] { key, value });

            var buffer = request.Format();

            CollectionAssert.AreEqual(expected, buffer.ToArray());
        }

        [TestMethod]
        public void Format_RequestWithKeyAndAbsoluteExpiration_WriteBuffer()
        {
            var command = "PEXPIREAT";
            var key = "foo";
            var time = new DateTimeOffset(2017, 8, 1, 10, 30, 10, TimeSpan.Zero);
            var expected = Resp.Encoding.GetBytes($"*3\r\n${command.Length}\r\n{command}\r\n${key.Length}\r\n{key}\r\n${time.ToUnixTimeMilliseconds().ToString().Length}\r\n{time.ToUnixTimeMilliseconds()}\r\n\r\n");
            var request = Mock.Of<IRequest>(r => r.Command == command && r.GetArgs() == new object[] { key, time });

            var buffer = request.Format();

            CollectionAssert.AreEqual(expected, buffer.ToArray());
        }

        [TestMethod]
        public void Format_RequestWithKeyAndSlidingExpiration_WriteBuffer()
        {
            var command = "PEXPIRE";
            var key = "foo";
            var timeSpan = TimeSpan.FromMilliseconds(18000000);
            var expected = Resp.Encoding.GetBytes($"*3\r\n${command.Length}\r\n{command}\r\n${key.Length}\r\n{key}\r\n${timeSpan.TotalMilliseconds.ToString("0").Length}\r\n{timeSpan.TotalMilliseconds:0}\r\n\r\n");
            var request = Mock.Of<IRequest>(r => r.Command == command && r.GetArgs() == new object[] { key, timeSpan });

            var buffer = request.Format();

            CollectionAssert.AreEqual(expected, buffer.ToArray());
        }

        [TestMethod]
        public async Task FormatAsync_RequestWithNoArgs_WriteBuffer()
        {
            var command = "DBSIZE";
            var expected = Resp.Encoding.GetBytes($"*1\r\n${command.Length}\r\n{command}\r\n\r\n");
            var request = Mock.Of<IRequest>(r => r.Command == command);

            var buffer = await request.FormatAsync(CancellationToken.None);

            CollectionAssert.AreEqual(expected, buffer.ToArray());
        }

        [TestMethod]
        public async Task FormatAsync_RequestWithKey_WriteBuffer()
        {
            var command = "GET";
            var key = "foo";
            var expected = Resp.Encoding.GetBytes($"*2\r\n${command.Length}\r\n{command}\r\n${key.Length}\r\n{key}\r\n\r\n");
            var request = Mock.Of<IRequest>(r => r.Command == command && r.GetArgs() == new object[] { key });

            var buffer = await request.FormatAsync(CancellationToken.None);

            CollectionAssert.AreEqual(expected, buffer.ToArray());
        }

        [TestMethod]
        public async Task FormatAsync_RequestWithKeyAndValue_WriteBuffer()
        {
            var command = "GETSET";
            var key = "foo";
            var value = "bar";
            var expected = Resp.Encoding.GetBytes($"*3\r\n${command.Length}\r\n{command}\r\n${key.Length}\r\n{key}\r\n${value.Length}\r\n{value}\r\n\r\n");
            var request = Mock.Of<IRequest>(r => r.Command == command && r.GetArgs() == new object[] { key, value });

            var buffer = await request.FormatAsync(CancellationToken.None);

            CollectionAssert.AreEqual(expected, buffer.ToArray());
        }

        [TestMethod]
        public async Task FormatAsync_RequestWithKeyAndAbsoluteExpiration_WriteBuffer()
        {
            var command = "PEXPIREAT";
            var key = "foo";
            var time = new DateTimeOffset(2017, 8, 1, 10, 30, 10, TimeSpan.Zero);
            var expected = Resp.Encoding.GetBytes($"*3\r\n${command.Length}\r\n{command}\r\n${key.Length}\r\n{key}\r\n${time.ToUnixTimeMilliseconds().ToString().Length}\r\n{time.ToUnixTimeMilliseconds()}\r\n\r\n");
            var request = Mock.Of<IRequest>(r => r.Command == command && r.GetArgs() == new object[] { key, time });

            var buffer = await request.FormatAsync(CancellationToken.None);

            CollectionAssert.AreEqual(expected, buffer.ToArray());
        }

        [TestMethod]
        public async Task FormatAsync_RequestWithKeyAndSlidingExpiration_WriteBuffer()
        {
            var command = "PEXPIRE";
            var key = "foo";
            var timeSpan = TimeSpan.FromMilliseconds(18000000);
            var expected = Resp.Encoding.GetBytes($"*3\r\n${command.Length}\r\n{command}\r\n${key.Length}\r\n{key}\r\n${timeSpan.TotalMilliseconds.ToString("0").Length}\r\n{timeSpan.TotalMilliseconds:0}\r\n\r\n");
            var request = Mock.Of<IRequest>(r => r.Command == command && r.GetArgs() == new object[] { key, timeSpan });

            var buffer = await request.FormatAsync(CancellationToken.None);

            CollectionAssert.AreEqual(expected, buffer.ToArray());
        }
    }
}