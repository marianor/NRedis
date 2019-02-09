using Framework.Caching.Redis.Transport;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Buffers;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Caching.Redis.Protocol.Tests
{
    [TestClass]
    public class RespClientTest
    {
        [TestMethod]
        public void RespClient_OptionsAccessorIsNull_Throws()
        {
            IOptions<RedisCacheOptions> optionsAccessor = null;
            var e = Assert.ThrowsException<ArgumentNullException>(() => new RespClient(optionsAccessor));

            Assert.AreEqual(e.ParamName, "optionsAccessor");
        }

        [TestMethod]
        public void Execute_RequestIsNull_Throws()
        {
            var target = new RespClient(new RedisCacheOptions { Host = "foo" });
            IRequest request = null;
            var e = Assert.ThrowsException<ArgumentNullException>(() => target.Execute(request));

            Assert.AreEqual(e.ParamName, "request");
        }

        [TestMethod]
        public void Execute_RequestsIsNull_Throws()
        {
            var target = new RespClient(new RedisCacheOptions { Host = "foo" });
            IRequest[] requests = null;
            var e = Assert.ThrowsException<ArgumentNullException>(() => target.Execute(requests));

            Assert.AreEqual(e.ParamName, "requests");
        }

        [TestMethod]
        public void Execute_RequestsIsEmpty_Throws()
        {
            var target = new RespClient(new RedisCacheOptions { Host = "foo" });
            var requests = Array.Empty<IRequest>();
            var e = Assert.ThrowsException<ArgumentException>(() => target.Execute(requests));

            Assert.AreEqual(e.ParamName, "requests");
        }

        [TestMethod]
        public async Task ExecuteAsync_RequestIsNull_Throws()
        {
            var target = new RespClient(new RedisCacheOptions { Host = "foo" });
            IRequest request = null;
            var e = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => target.ExecuteAsync(request));

            Assert.AreEqual(e.ParamName, "request");
        }

        [TestMethod]
        public async Task ExecuteAsync_RequestsIsNull_Throws()
        {
            var target = new RespClient(new RedisCacheOptions { Host = "foo" });
            IRequest[] requests = null;
            var e = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => target.ExecuteAsync(requests));

            Assert.AreEqual(e.ParamName, "requests");
        }

        [TestMethod]
        public async Task ExecuteAsync_RequestsIsEmpty_Throws()
        {
            var target = new RespClient(new RedisCacheOptions { Host = "foo" });
            var requests = Array.Empty<IRequest>();
            var e = await Assert.ThrowsExceptionAsync<ArgumentException>(() => target.ExecuteAsync(requests));

            Assert.AreEqual(e.ParamName, "requests");
        }

        [TestMethod]
        public void Execute_WithTransportConnectionClosed_InvokesConnect()
        {
            var transportMock = new Mock<ITransport>();
            transportMock.Setup(t => t.State).Returns(TransportState.Closed);
            transportMock.Setup(t => t.Send(It.IsAny<ReadOnlySequence<byte>>()))
                .Returns(new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes($"+foo\r\n")));

            var target = new RespClient(new RedisCacheOptions { Host = "foo" }, transportMock.Object);
            var request = Mock.Of<IRequest>(r => r.Command == "foo" && r.GetArgs() == Array.Empty<object>());

            var response = target.Execute(request);

            Assert.IsNotNull(response);
            transportMock.Verify(t => t.Connect(), Times.Once());
            transportMock.Verify(t => t.Send(It.IsAny<ReadOnlySequence<byte>>()), Times.Once());
        }

        [TestMethod]
        public void Execute_WithValidRequest_InvokesSend()
        {
            var transportMock = new Mock<ITransport>();
            transportMock.Setup(t => t.State).Returns(TransportState.Connected);
            transportMock.Setup(t => t.Send(It.IsAny<ReadOnlySequence<byte>>()))
                .Returns(new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes($"+foo\r\n")));

            var target = new RespClient(new RedisCacheOptions { Host = "foo" }, transportMock.Object);
            var request = Mock.Of<IRequest>(r => r.Command == "foo" && r.GetArgs() == Array.Empty<object>());

            var response = target.Execute(request);

            Assert.IsNotNull(response);
            transportMock.Verify(t => t.Send(It.IsAny<ReadOnlySequence<byte>>()), Times.Once());
        }

        [TestMethod]
        public void ExecuteAsync_WithTransportConnectionAsyncClosed_InvokesConnectAsync()
        {
            var transportMock = new Mock<ITransport>();
            transportMock.Setup(t => t.State).Returns(TransportState.Closed);
            transportMock.Setup(t => t.SendAsync(It.IsAny<ReadOnlySequence<byte>>(), default))
                .ReturnsAsync(new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes($"+foo\r\n")));

            var target = new RespClient(new RedisCacheOptions { Host = "foo" }, transportMock.Object);
            var request = Mock.Of<IRequest>(r => r.Command == "foo" && r.GetArgs() == Array.Empty<object>());

            var response = target.ExecuteAsync(request);

            Assert.IsNotNull(response);
            transportMock.Verify(t => t.ConnectAsync(default), Times.Once());
            transportMock.Verify(t => t.SendAsync(It.IsAny<ReadOnlySequence<byte>>(), default), Times.Once());
        }

        [TestMethod]
        public async Task ExecuteAsync_WithValidRequest_InvokesSendAsync()
        {
            var transportMock = new Mock<ITransport>();
            transportMock.Setup(t => t.State).Returns(TransportState.Connected);
            transportMock.Setup(t => t.SendAsync(It.IsAny<ReadOnlySequence<byte>>(), default))
                .ReturnsAsync(new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes($"+foo\r\n")));

            var target = new RespClient(new RedisCacheOptions { Host = "foo", Password = "A" }, transportMock.Object);
            var request = Mock.Of<IRequest>(r => r.Command == "foo" && r.GetArgs() == Array.Empty<object>());

            var response = await target.ExecuteAsync(request);

            Assert.IsNotNull(response);
            transportMock.Verify(t => t.SendAsync(It.IsAny<ReadOnlySequence<byte>>(), default), Times.Once());
        }
    }
}