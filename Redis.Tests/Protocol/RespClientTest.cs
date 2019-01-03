using Framework.Caching.Transport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Net;
using System.Text;

namespace Framework.Caching.Protocol.Tests
{
    [TestClass]
    public class RespClientTest
    {
        [TestMethod]
        public void RespClient_ThrowsIfClientNull()
        {
            var e = Assert.ThrowsException<ArgumentNullException>(() => new RespClient(null));
            Assert.AreEqual(e.ParamName, "settings");
        }

        [TestMethod]
        public void Execute_ThrowsIfRequestsNull()
        {
            var target = new RespClient(Mock.Of<ITransportSettings>(t => t.CreateTransport() == Mock.Of<ITransport>()));
            var e = Assert.ThrowsException<ArgumentNullException>(() => target.Execute(null));
            Assert.AreEqual(e.ParamName, "requests");
        }

        [TestMethod]
        public void Execute_ThrowsIfRequestsEmpty()
        {
            var target = new RespClient(Mock.Of<ITransportSettings>(t => t.CreateTransport() == Mock.Of<ITransport>()));
            var e = Assert.ThrowsException<ArgumentException>(() => target.Execute());
            Assert.AreEqual(e.ParamName, "requests");
        }

        [TestMethod]
        public void Execute_ParseThrowsIfInvalidResponse()
        {
            var transport = Mock.Of<ITransport>(t => t.Send(It.IsAny<byte[]>()) == Encoding.UTF8.GetBytes("|3\r\n$3\r\nfoo\r\n$-1\r\n$3\r\nbar\r\n"));
            var request = Mock.Of<IRequest>();
            var target = new RespClient(Mock.Of<ITransportSettings>(t => t.CreateTransport() == transport));

            Assert.ThrowsException<ProtocolViolationException>(() => target.Execute(request));
        }

        [TestMethod]
        public void Execute_ParseEmptySimpleString()
        {
            var transport = Mock.Of<ITransport>(t => t.Send(It.IsAny<byte[]>()) == Encoding.UTF8.GetBytes("+\r\n"));
            var target = new RespClient(Mock.Of<ITransportSettings>(t => t.CreateTransport() == transport));
            var request = Mock.Of<IRequest>();

            var responses = target.Execute(request);

            Assert.AreEqual(1, responses.Length);
            Assert.IsInstanceOfType(responses[0], typeof(StringResponse));
            Assert.AreEqual(ValueType.SimpleString, responses[0].ValueType);
            Assert.AreEqual(string.Empty, responses[0].Value);
        }

        [TestMethod]
        public void Execute_ParseSimpleString()
        {
            var transport = Mock.Of<ITransport>(t => t.Send(It.IsAny<byte[]>()) == Encoding.UTF8.GetBytes("+foo\r\n"));
            var target = new RespClient(Mock.Of<ITransportSettings>(t => t.CreateTransport() == transport));
            var request = Mock.Of<IRequest>();

            var responses = target.Execute(request);

            Assert.AreEqual(1, responses.Length);
            Assert.IsInstanceOfType(responses[0], typeof(StringResponse));
            Assert.AreEqual(ValueType.SimpleString, responses[0].ValueType);
            Assert.AreEqual("foo", responses[0].Value);
        }

        [TestMethod]
        public void Execute_ParseSimpleStringThrowsIfInvalidEnd()
        {
            var transport = Mock.Of<ITransport>(t => t.Send(It.IsAny<byte[]>()) == Encoding.UTF8.GetBytes("+foo"));
            var target = new RespClient(Mock.Of<ITransportSettings>(t => t.CreateTransport() == transport));
            var request = Mock.Of<IRequest>();

            Assert.ThrowsException<ProtocolViolationException>(() => target.Execute(request));
        }

        [TestMethod]
        public void Execute_ParseSimpleStringThrowsIfInvalidEndSequence()
        {
            var transport = Mock.Of<ITransport>(t => t.Send(It.IsAny<byte[]>()) == Encoding.UTF8.GetBytes("+foo\r "));
            var target = new RespClient(Mock.Of<ITransportSettings>(t => t.CreateTransport() == transport));
            var request = Mock.Of<IRequest>();

            Assert.ThrowsException<ProtocolViolationException>(() => target.Execute(request));
        }

        [TestMethod]
        public void Execute_ParseError()
        {
            var transport = Mock.Of<ITransport>(t => t.Send(It.IsAny<byte[]>()) == Encoding.UTF8.GetBytes("-foo\r\n"));
            var target = new RespClient(Mock.Of<ITransportSettings>(t => t.CreateTransport() == transport));
            var request = Mock.Of<IRequest>();

            var responses = target.Execute(request);

            Assert.AreEqual(1, responses.Length);
            Assert.IsInstanceOfType(responses[0], typeof(StringResponse));
            Assert.AreEqual(ValueType.Error, responses[0].ValueType);
            Assert.AreEqual("foo", responses[0].Value);
        }

        [TestMethod]
        public void Execute_ParseErrorThrowsIfInvalidEnd()
        {
            var transport = Mock.Of<ITransport>(t => t.Send(It.IsAny<byte[]>()) == Encoding.UTF8.GetBytes("-foo"));
            var target = new RespClient(Mock.Of<ITransportSettings>(t => t.CreateTransport() == transport));
            var request = Mock.Of<IRequest>();

            Assert.ThrowsException<ProtocolViolationException>(() => target.Execute(request));
        }

        [TestMethod]
        public void Execute_ParseErrorThrowsIfInvalidEndSequence()
        {
            var transport = Mock.Of<ITransport>(t => t.Send(It.IsAny<byte[]>()) == Encoding.UTF8.GetBytes("-foo\r "));
            var target = new RespClient(Mock.Of<ITransportSettings>(t => t.CreateTransport() == transport));
            var request = Mock.Of<IRequest>();

            Assert.ThrowsException<ProtocolViolationException>(() => target.Execute(request));
        }

        [TestMethod]
        public void Execute_ParseInteger()
        {
            var transport = Mock.Of<ITransport>(t => t.Send(It.IsAny<byte[]>()) == Encoding.UTF8.GetBytes(":34\r\n"));
            var request = Mock.Of<IRequest>();
            var target = new RespClient(Mock.Of<ITransportSettings>(t => t.CreateTransport() == transport));

            var responses = target.Execute(request);

            Assert.AreEqual(1, responses.Length);
            Assert.IsInstanceOfType(responses[0], typeof(IntegerResponse));
            Assert.AreEqual(ValueType.Integer, responses[0].ValueType);
            Assert.AreEqual(34, responses[0].Value);
        }

        [TestMethod]
        public void Execute_ParseIntegerThrowsIfInvalidEnd()
        {
            var transport = Mock.Of<ITransport>(t => t.Send(It.IsAny<byte[]>()) == Encoding.UTF8.GetBytes(":34"));
            var request = Mock.Of<IRequest>();
            var target = new RespClient(Mock.Of<ITransportSettings>(t => t.CreateTransport() == transport));

            Assert.ThrowsException<ProtocolViolationException>(() => target.Execute(request));
        }

        [TestMethod]
        public void Execute_ParseIntegerThrowsIfInvalidEndSequence()
        {
            var transport = Mock.Of<ITransport>(t => t.Send(It.IsAny<byte[]>()) == Encoding.UTF8.GetBytes(":34\r "));
            var target = new RespClient(Mock.Of<ITransportSettings>(t => t.CreateTransport() == transport));
            var request = Mock.Of<IRequest>();

            Assert.ThrowsException<ProtocolViolationException>(() => target.Execute(request));
        }

        [TestMethod]
        public void Execute_ParseBulkString()
        {
            var transport = Mock.Of<ITransport>(t => t.Send(It.IsAny<byte[]>()) == Encoding.UTF8.GetBytes("$3\r\nfoo\r\n"));
            var target = new RespClient(Mock.Of<ITransportSettings>(t => t.CreateTransport() == transport));
            var request = Mock.Of<IRequest>();

            var responses = target.Execute(request);

            Assert.AreEqual(1, responses.Length);
            Assert.IsInstanceOfType(responses[0], typeof(StringResponse));
            Assert.AreEqual(ValueType.BulkString, responses[0].ValueType);
            Assert.AreEqual("foo", responses[0].Value);
        }

        [TestMethod]
        public void Execute_ParseBulkStringThrowsIfInvalidEnd()
        {
            var transport = Mock.Of<ITransport>(t => t.Send(It.IsAny<byte[]>()) == Encoding.UTF8.GetBytes("$3\r\nfoo"));
            var target = new RespClient(Mock.Of<ITransportSettings>(t => t.CreateTransport() == transport));
            var request = Mock.Of<IRequest>();

            Assert.ThrowsException<ProtocolViolationException>(() => target.Execute(request));
        }

        [TestMethod]
        public void Execute_ParseBulkStringThrowsIfInvalidEndSequence()
        {
            var transport = Mock.Of<ITransport>(t => t.Send(It.IsAny<byte[]>()) == Encoding.UTF8.GetBytes("$3\r\nfoo\r "));
            var target = new RespClient(Mock.Of<ITransportSettings>(t => t.CreateTransport() == transport));
            var request = Mock.Of<IRequest>();

            Assert.ThrowsException<ProtocolViolationException>(() => target.Execute(request));
        }

        [TestMethod]
        public void Execute_ParseEmptyBulkString()
        {
            var transport = Mock.Of<ITransport>(t => t.Send(It.IsAny<byte[]>()) == Encoding.UTF8.GetBytes("$0\r\n\r\n"));
            var target = new RespClient(Mock.Of<ITransportSettings>(t => t.CreateTransport() == transport));
            var request = Mock.Of<IRequest>();

            var responses = target.Execute(request);

            Assert.AreEqual(1, responses.Length);
            Assert.IsInstanceOfType(responses[0], typeof(StringResponse));
            Assert.AreEqual(ValueType.BulkString, responses[0].ValueType);
            Assert.AreEqual(string.Empty, responses[0].Value);
        }

        [TestMethod]
        public void Execute_ParseNullBulkString()
        {
            var transport = Mock.Of<ITransport>(t => t.Send(It.IsAny<byte[]>()) == Encoding.UTF8.GetBytes("$-1\r\n"));
            var target = new RespClient(Mock.Of<ITransportSettings>(t => t.CreateTransport() == transport));
            var request = Mock.Of<IRequest>();

            var responses = target.Execute(request);

            Assert.AreEqual(1, responses.Length);
            Assert.IsInstanceOfType(responses[0], typeof(StringResponse));
            Assert.AreEqual(ValueType.BulkString, responses[0].ValueType);
            Assert.IsNull(responses[0].Value);
        }

        [TestMethod]
        public void Execute_ParseMultilineBulkString()
        {
            var transport = Mock.Of<ITransport>(t => t.Send(It.IsAny<byte[]>()) == Encoding.UTF8.GetBytes("$8\r\nfoo\r\nbar\r\n"));
            var target = new RespClient(Mock.Of<ITransportSettings>(t => t.CreateTransport() == transport));
            var request = Mock.Of<IRequest>();

            var responses = target.Execute(request);

            Assert.AreEqual(1, responses.Length);
            Assert.IsInstanceOfType(responses[0], typeof(StringResponse));
            Assert.AreEqual(ValueType.BulkString, responses[0].ValueType);
            Assert.AreEqual("foo\r\nbar", responses[0].Value);
        }

        [TestMethod]
        public void Execute_ParseEmptyArray()
        {
            var transport = Mock.Of<ITransport>(t => t.Send(It.IsAny<byte[]>()) == Encoding.UTF8.GetBytes("*0\r\n"));
            var target = new RespClient(Mock.Of<ITransportSettings>(t => t.CreateTransport() == transport));
            var request = Mock.Of<IRequest>();

            var responses = target.Execute(request);

            Assert.AreEqual(1, responses.Length);
            Assert.IsInstanceOfType(responses[0], typeof(ArrayResponse));
            Assert.AreEqual(ValueType.Array, responses[0].ValueType);
            Assert.AreEqual(0, ((object[])responses[0].Value).Length);
        }

        [TestMethod]
        public void Execute_ParseEmptyArrayThrowsIfInvalidEnd()
        {
            var transport = Mock.Of<ITransport>(t => t.Send(It.IsAny<byte[]>()) == Encoding.UTF8.GetBytes("*0"));
            var target = new RespClient(Mock.Of<ITransportSettings>(t => t.CreateTransport() == transport));
            var request = Mock.Of<IRequest>();

            Assert.ThrowsException<ProtocolViolationException>(() => target.Execute(request));
        }


        [TestMethod]
        public void Execute_ParseEmptyArrayThrowsIfInvalidEndSequence()
        {
            var transport = Mock.Of<ITransport>(t => t.Send(It.IsAny<byte[]>()) == Encoding.UTF8.GetBytes("*0\r "));
            var target = new RespClient(Mock.Of<ITransportSettings>(t => t.CreateTransport() == transport));
            var request = Mock.Of<IRequest>();

            Assert.ThrowsException<ProtocolViolationException>(() => target.Execute(request));
        }

        [TestMethod]
        public void Execute_ParseNullArray()
        {
            var transport = Mock.Of<ITransport>(t => t.Send(It.IsAny<byte[]>()) == Encoding.UTF8.GetBytes("*-1\r\n"));
            var target = new RespClient(Mock.Of<ITransportSettings>(t => t.CreateTransport() == transport));
            var request = Mock.Of<IRequest>();

            var responses = target.Execute(request);

            Assert.AreEqual(1, responses.Length);
            Assert.IsInstanceOfType(responses[0], typeof(ArrayResponse));
            Assert.AreEqual(ValueType.Array, responses[0].ValueType);
            Assert.IsNull(responses[0].Value);
        }

        [TestMethod]
        public void Execute_ParseIntegerArray()
        {
            var transport = Mock.Of<ITransport>(t => t.Send(It.IsAny<byte[]>()) == Encoding.UTF8.GetBytes("*1\r\n:10\r\n"));
            var target = new RespClient(Mock.Of<ITransportSettings>(t => t.CreateTransport() == transport));
            var request = Mock.Of<IRequest>();

            var responses = target.Execute(request);

            Assert.AreEqual(1, responses.Length);
            Assert.IsInstanceOfType(responses[0], typeof(ArrayResponse));
            Assert.AreEqual(ValueType.Array, responses[0].ValueType);

            var result = (object[])responses[0].Value;
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(10, result[0]);
        }

        [TestMethod]
        public void Execute_ParseBulkStringArray()
        {
            var transport = Mock.Of<ITransport>(t => t.Send(It.IsAny<byte[]>()) == Encoding.UTF8.GetBytes("*2\r\n$3\r\nfoo\r\n$3\r\nbar\r\n"));
            var target = new RespClient(Mock.Of<ITransportSettings>(t => t.CreateTransport() == transport));
            var request = Mock.Of<IRequest>();

            var responses = target.Execute(request);

            Assert.AreEqual(1, responses.Length);
            Assert.IsInstanceOfType(responses[0], typeof(ArrayResponse));
            Assert.AreEqual(ValueType.Array, responses[0].ValueType);

            var result = (object[])responses[0].Value;
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("foo", result[0]);
            Assert.AreEqual("bar", result[1]);
        }

        [TestMethod]
        public void Execute_ParseSimpleStringArray()
        {
            var transport = Mock.Of<ITransport>(t => t.Send(It.IsAny<byte[]>()) == Encoding.UTF8.GetBytes("*1\r\n+foo\r\n"));
            var target = new RespClient(Mock.Of<ITransportSettings>(t => t.CreateTransport() == transport));
            var request = Mock.Of<IRequest>();

            var responses = target.Execute(request);

            Assert.AreEqual(1, responses.Length);
            Assert.IsInstanceOfType(responses[0], typeof(ArrayResponse));
            Assert.AreEqual(ValueType.Array, responses[0].ValueType);

            var result = (object[])responses[0].Value;
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("foo", result[0]);
        }

        [TestMethod]
        public void Execute_ParseMixedTypeArray()
        {
            var transport = Mock.Of<ITransport>(t => t.Send(It.IsAny<byte[]>()) == Encoding.UTF8.GetBytes("*5\r\n:1\r\n:2\r\n:3\r\n:4\r\n$6\r\nfoobar\r\n"));
            var target = new RespClient(Mock.Of<ITransportSettings>(t => t.CreateTransport() == transport));
            var request = Mock.Of<IRequest>();

            var responses = target.Execute(request);

            Assert.AreEqual(1, responses.Length);
            Assert.IsInstanceOfType(responses[0], typeof(ArrayResponse));
            Assert.AreEqual(ValueType.Array, responses[0].ValueType);

            var result = (object[])responses[0].Value;
            Assert.AreEqual(5, result.Length);
            Assert.AreEqual(1, result[0]);
            Assert.AreEqual(2, result[1]);
            Assert.AreEqual(3, result[2]);
            Assert.AreEqual(4, result[3]);
            Assert.AreEqual("foobar", result[4]);
        }

        [TestMethod]
        public void Execute_ParseArrayOfArrays()
        {
            var transport = Mock.Of<ITransport>(t => t.Send(It.IsAny<byte[]>()) == Encoding.UTF8.GetBytes("*2\r\n*3\r\n:1\r\n:2\r\n:3\r\n*2\r\n+Foo\r\n-Bar\r\n"));
            var target = new RespClient(Mock.Of<ITransportSettings>(t => t.CreateTransport() == transport));
            var request = Mock.Of<IRequest>();

            var responses = target.Execute(request);

            Assert.AreEqual(1, responses.Length);
            Assert.IsInstanceOfType(responses[0], typeof(ArrayResponse));
            Assert.AreEqual(ValueType.Array, responses[0].ValueType);

            var result = (object[])responses[0].Value;
            Assert.AreEqual(2, result.Length);

            var innerResult = (object[])result[0];
            Assert.AreEqual(1, innerResult[0]);
            Assert.AreEqual(2, innerResult[1]);
            Assert.AreEqual(3, innerResult[2]);

            innerResult = (object[])result[1];
            Assert.AreEqual("Foo", innerResult[0]);
            Assert.AreEqual("Bar", innerResult[1]);
        }

        [TestMethod]
        public void Execute_NullElementsInArray()
        {
            var transport = Mock.Of<ITransport>(t => t.Send(It.IsAny<byte[]>()) == Encoding.UTF8.GetBytes("*3\r\n$3\r\nfoo\r\n$-1\r\n$3\r\nbar\r\n"));
            var target = new RespClient(Mock.Of<ITransportSettings>(t => t.CreateTransport() == transport));
            var request = Mock.Of<IRequest>();

            var responses = target.Execute(request);

            Assert.AreEqual(1, responses.Length);
            Assert.IsInstanceOfType(responses[0], typeof(ArrayResponse));
            Assert.AreEqual(ValueType.Array, responses[0].ValueType);

            var result = (object[])responses[0].Value;
            Assert.AreEqual(3, result.Length);
            Assert.AreEqual("foo", result[0]);
            Assert.IsNull(result[1]);
            Assert.AreEqual("bar", result[2]);
            Assert.AreEqual(ValueType.Array, responses[0].ValueType);
        }
    }
}