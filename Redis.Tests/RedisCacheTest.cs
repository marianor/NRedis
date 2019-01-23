using Framework.Caching.Redis.Protocol;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DataType = Framework.Caching.Redis.Protocol.DataType;

namespace Framework.Caching.Redis.Tests
{
    [TestClass]
    public class RedisCacheTest
    {
        [TestMethod]
        public void RedisCache_OptionsAccessorIsNull_Throws()
        {
            RedisCacheOptions optionsAccessor = null;

            var e = Assert.ThrowsException<ArgumentNullException>(() => new RedisCache(optionsAccessor));
            Assert.AreEqual("optionsAccessor", e.ParamName);
        }

        [TestMethod]
        public void Get_KeyIsNull_Throws()
        {
            var target = new RedisCache(new RedisCacheOptions { Host = "foo" });

            var e = Assert.ThrowsException<ArgumentNullException>(() => target.Get(null));
            Assert.AreEqual(e.ParamName, "key");
        }

        [TestMethod]
        public void Get_KeyIsEmpty_Throws()
        {
            var target = new RedisCache(new RedisCacheOptions(), Mock.Of<IRespClient>());

            var e = Assert.ThrowsException<ArgumentException>(() => target.Get(string.Empty));
            Assert.AreEqual(e.ParamName, "key");
        }

        [TestMethod]
        public void Get_ExistingKey_ReturnsValue()
        {
            var expectedKey = "foo";
            var expectedValue = RespProtocol.Encoding.GetBytes("bar");
            var response = Mock.Of<IResponse>(r => r.GetRawValue() == expectedValue);
            var respClientMock = new Mock<IRespClient>();
            respClientMock.Setup(c => c.Execute(It.IsAny<IRequest>())).Returns(response);
            var target = new RedisCache(new RedisCacheOptions(), respClientMock.Object);

            var value = target.Get(expectedKey);

            CollectionAssert.AreEqual(expectedValue, value);
            IRequest request = null;
            Func<IRequest, bool> assert = r =>
            {
                request = r;
                return r != null;
            };
            respClientMock.Verify(c => c.Execute(It.Is<IRequest>(r => assert(r))), Times.Once());
            Assert.AreEqual("HGET", request.Command);
            Assert.AreEqual(2, request.GetArgs().Length);
            Assert.AreEqual(expectedKey, request.GetArg<string>(0));
            CollectionAssert.AreEqual(RespProtocol.Encoding.GetBytes("val"), request.GetArg<byte[]>(1));
        }

        [TestMethod]
        public async Task GetAsync_KeyIsNull_Throws()
        {
            var target = new RedisCache(new RedisCacheOptions(), Mock.Of<IRespClient>());

            var e = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => target.GetAsync(null));
            Assert.AreEqual(e.ParamName, "key");
        }

        [TestMethod]
        public async Task GetAsync_KeyIsEmpty_Throws()
        {
            var target = new RedisCache(new RedisCacheOptions(), Mock.Of<IRespClient>());

            var e = await Assert.ThrowsExceptionAsync<ArgumentException>(() => target.GetAsync(string.Empty));
            Assert.AreEqual(e.ParamName, "key");
        }

        [TestMethod]
        public async Task GetAsync_ExistingKey_ReturnsValue()
        {
            var expectedKey = "foo";
            var expectedValue = RespProtocol.Encoding.GetBytes("bar");
            var response = Mock.Of<IResponse>(r => r.GetRawValue() == expectedValue);
            var respClientMock = new Mock<IRespClient>();
            respClientMock.Setup(c => c.ExecuteAsync(It.IsAny<IRequest>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(response));
            var target = new RedisCache(new RedisCacheOptions(), respClientMock.Object);

            var value = await target.GetAsync(expectedKey);

            CollectionAssert.AreEqual(expectedValue, value);
            IRequest request = null;
            Func<IRequest, bool> assert = r =>
            {
                request = r;
                return r != null;
            };
            respClientMock.Verify(c => c.ExecuteAsync(It.Is<IRequest>(r => assert(r)), It.IsAny<CancellationToken>()), Times.Once());
            Assert.AreEqual("HGET", request.Command);
            Assert.AreEqual(2, request.GetArgs().Length);
            Assert.AreEqual(expectedKey, request.GetArg<string>(0));
            CollectionAssert.AreEqual(RespProtocol.Encoding.GetBytes("val"), request.GetArg<byte[]>(1));
        }

        [TestMethod]
        public void Refresh_KeyIsNull_Throws()
        {
            var target = new RedisCache(new RedisCacheOptions(), Mock.Of<IRespClient>());

            var e = Assert.ThrowsException<ArgumentNullException>(() => target.Refresh(null));
            Assert.AreEqual("key", e.ParamName);
        }

        [TestMethod]
        public void Refresh_KeyIsEmpty_Throws()
        {
            var target = new RedisCache(new RedisCacheOptions(), Mock.Of<IRespClient>());

            var e = Assert.ThrowsException<ArgumentException>(() => target.Refresh(string.Empty));
            Assert.AreEqual("key", e.ParamName);
        }

        [TestMethod]
        public async Task RefreshAsync_KeyIsNull_Throws()
        {
            var target = new RedisCache(new RedisCacheOptions(), Mock.Of<IRespClient>());

            var e = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => target.RefreshAsync(null));
            Assert.AreEqual("key", e.ParamName);
        }

        [TestMethod]
        public async Task RefreshAsync_KeyIsEmpty_Throws()
        {
            var target = new RedisCache(new RedisCacheOptions(), Mock.Of<IRespClient>());

            var e = await Assert.ThrowsExceptionAsync<ArgumentException>(() => target.RefreshAsync(string.Empty));
            Assert.AreEqual("key", e.ParamName);
        }

        [TestMethod]
        public async Task RefreshAsync_ErrorResponse_Trows()
        {
            var expectedMessage = "bar";
            var response = Mock.Of<IResponse>(r => r.DataType == DataType.Error && r.Value == (object)expectedMessage);
            var respClientMock = new Mock<IRespClient>();
            respClientMock.Setup(c => c.ExecuteAsync(It.IsAny<IRequest>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(response));
            var target = new RedisCache(new RedisCacheOptions(), respClientMock.Object);
            var expectedKey = "foo";

            var e = await Assert.ThrowsExceptionAsync<ProtocolViolationException>(() => target.RefreshAsync(expectedKey));
            Assert.AreEqual(expectedMessage, e.Message);
        }


        [TestMethod]
        public async Task RefreshAsync_Key_InvokeExecute()
        {
            var response = Mock.Of<IResponse>(r => r.DataType == DataType.SimpleString);
            var respClientMock = new Mock<IRespClient>();
            respClientMock.Setup(c => c.ExecuteAsync(It.IsAny<IRequest>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(response));
            var target = new RedisCache(new RedisCacheOptions(), respClientMock.Object);
            var expectedKey = "foo";

            await target.RefreshAsync(expectedKey);

            IRequest request = null;
            Func<IRequest, bool> assert = r =>
            {
                request = r;
                return r != null;
            };
            respClientMock.Verify(c => c.ExecuteAsync(It.Is<IRequest>(r => assert(r)), It.IsAny<CancellationToken>()), Times.Once());
            Assert.AreEqual("EVAL", request.Command);
            Assert.AreEqual(4, request.GetArgs().Length);
            StringAssert.Contains(RespProtocol.Encoding.GetString(request.GetArg<byte[]>(0)), "PEXPIRE");
            Assert.AreEqual(1, request.GetArg<int>(1));
            Assert.AreEqual(expectedKey, request.GetArg<string>(2));
            CollectionAssert.AreEqual(RespProtocol.Encoding.GetBytes("sld"), request.GetArg<byte[]>(3));
        }

        [TestMethod]
        public void Remove_KeyIsNull_Throws()
        {
            var target = new RedisCache(new RedisCacheOptions(), Mock.Of<IRespClient>());

            var e = Assert.ThrowsException<ArgumentNullException>(() => target.Remove(null));
            Assert.AreEqual("key", e.ParamName);
        }

        [TestMethod]
        public void Remove_KeyIsEmpty_Throws()
        {
            var target = new RedisCache(new RedisCacheOptions(), Mock.Of<IRespClient>());

            var e = Assert.ThrowsException<ArgumentException>(() => target.Remove(string.Empty));
            Assert.AreEqual("key", e.ParamName);
        }

        [TestMethod]
        public void Remove_Key_InvokeExecute()
        {
            var respClientMock = new Mock<IRespClient>();
            var target = new RedisCache(new RedisCacheOptions(), respClientMock.Object);
            var expectedKey = "foo";

            target.Remove(expectedKey);

            IRequest request = null;
            Func<IRequest, bool> assert = r =>
            {
                request = r;
                return r != null;
            };
            respClientMock.Verify(c => c.Execute(It.Is<IRequest>(r => assert(r))), Times.Once());
            Assert.AreEqual("DEL", request.Command);
            Assert.AreEqual(1, request.GetArgs().Length);
            Assert.AreEqual(expectedKey, request.GetArg<string>(0));
        }

        [TestMethod]
        public async Task RemoveAsync_KeyIsNull_Throws()
        {
            var target = new RedisCache(new RedisCacheOptions(), Mock.Of<IRespClient>());

            var e = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => target.RemoveAsync(null));
            Assert.AreEqual("key", e.ParamName);
        }

        [TestMethod]
        public async Task RemoveAsync_KeyIsEmpty_Throws()
        {
            var target = new RedisCache(new RedisCacheOptions(), Mock.Of<IRespClient>());

            var e = await Assert.ThrowsExceptionAsync<ArgumentException>(() => target.RemoveAsync(string.Empty));
            Assert.AreEqual("key", e.ParamName);
        }

        [TestMethod]
        public async Task RemoveAsync_Key_InvokeExecute()
        {
            var respClientMock = new Mock<IRespClient>();
            var target = new RedisCache(new RedisCacheOptions(), respClientMock.Object);
            var expectedKey = "foo";

            await target.RemoveAsync(expectedKey);

            IRequest request = null;
            Func<IRequest, bool> assert = r =>
            {
                request = r;
                return r != null;
            };
            respClientMock.Verify(c => c.ExecuteAsync(It.Is<IRequest>(r => assert(r)), It.IsAny<CancellationToken>()), Times.Once());
            Assert.AreEqual("DEL", request.Command);
            Assert.AreEqual(1, request.GetArgs().Length);
            Assert.AreEqual(expectedKey, request.GetArg<string>(0));
        }

        [TestMethod]
        public void Set_KeyIsNull_Throws()
        {
            var target = new RedisCache(new RedisCacheOptions(), Mock.Of<IRespClient>());

            var e = Assert.ThrowsException<ArgumentNullException>(() => target.Set(null, new byte[] { 65 }, new DistributedCacheEntryOptions()));
            Assert.AreEqual(e.ParamName, "key");
        }

        [TestMethod]
        public void Set_KeyIsEmpty_Throws()
        {
            var target = new RedisCache(new RedisCacheOptions(), Mock.Of<IRespClient>());

            var e = Assert.ThrowsException<ArgumentException>(() => target.Set(string.Empty, new byte[] { 65 }, new DistributedCacheEntryOptions()));
            Assert.AreEqual(e.ParamName, "key");
        }

        [TestMethod]
        public void Set_ValueIsNull_Throws()
        {
            var target = new RedisCache(new RedisCacheOptions(), Mock.Of<IRespClient>());

            var e = Assert.ThrowsException<ArgumentNullException>(() => target.Set("foo", null, new DistributedCacheEntryOptions()));
            Assert.AreEqual(e.ParamName, "value");
        }

        [TestMethod]
        public void Set_OptionsIsNull_Throws()
        {
            var target = new RedisCache(new RedisCacheOptions(), Mock.Of<IRespClient>());

            var e = Assert.ThrowsException<ArgumentNullException>(() => target.Set("foo", new byte[] { 65 }, null));
            Assert.AreEqual(e.ParamName, "options");
        }

        [TestMethod]
        public void Set_KeyAndValue_InvokeExecute()
        {
            var respClientMock = new Mock<IRespClient>();
            var target = new RedisCache(new RedisCacheOptions(), respClientMock.Object);
            var expectedKey = "foo";
            var expectedValue = RespProtocol.Encoding.GetBytes("bar");

            target.Set(expectedKey, expectedValue);

            IRequest request = null;
            Func<IRequest, bool> assert = r =>
            {
                request = r;
                return r != null;
            };
            respClientMock.Verify(c => c.Execute(It.Is<IRequest>(r => assert(r))), Times.Once());
            Assert.AreEqual("HMSET", request.Command);
            Assert.AreEqual(5, request.GetArgs().Length);
            Assert.AreEqual(expectedKey, request.GetArg<string>(0));
            CollectionAssert.AreEqual(RespProtocol.Encoding.GetBytes("val"), request.GetArg<byte[]>(1));
            CollectionAssert.AreEqual(expectedValue, request.GetArg<byte[]>(2));
            CollectionAssert.AreEqual(RespProtocol.Encoding.GetBytes("sld"), request.GetArg<byte[]>(3));
            Assert.AreEqual(TimeSpan.Zero, request.GetArg<TimeSpan>(4));
        }

        [TestMethod]
        public void Set_KeyValueAndAbsoluteExpiration_InvokeExecute()
        {
            var respClientMock = new Mock<IRespClient>();
            var target = new RedisCache(new RedisCacheOptions(), respClientMock.Object);
            var expectedKey = "foo";
            var expectedValue = RespProtocol.Encoding.GetBytes("bar");
            var expectedExpiration = DateTimeOffset.Now.AddHours(1);

            target.Set(expectedKey, expectedValue, new DistributedCacheEntryOptions { AbsoluteExpiration = expectedExpiration });

            IRequest[] requests = null;
            Func<IEnumerable<IRequest>, bool> assert = r =>
            {
                requests = r.ToArray();
                return r != null;
            };
            respClientMock.Verify(c => c.Execute(It.Is<IEnumerable<IRequest>>(r => assert(r))), Times.Once());
            Assert.AreEqual("HMSET", requests[0].Command);
            Assert.AreEqual(5, requests[0].GetArgs().Length);
            Assert.AreEqual(expectedKey, requests[0].GetArg<string>(0));
            CollectionAssert.AreEqual(RespProtocol.Encoding.GetBytes("val"), requests[0].GetArg<byte[]>(1));
            CollectionAssert.AreEqual(expectedValue, requests[0].GetArg<byte[]>(2));
            CollectionAssert.AreEqual(RespProtocol.Encoding.GetBytes("sld"), requests[0].GetArg<byte[]>(3));
            Assert.AreEqual(TimeSpan.Zero, requests[0].GetArg<TimeSpan>(4));
            Assert.AreEqual("PEXPIREAT", requests[1].Command);
            Assert.AreEqual(2, requests[1].GetArgs().Length);
            Assert.AreEqual(expectedKey, requests[1].GetArg<string>(0));
            Assert.AreEqual(expectedExpiration, requests[1].GetArg<DateTimeOffset>(1));
        }

        [TestMethod]
        public void Set_KeyValueAndSlidingExpiration_InvokeExecute()
        {
            var respClientMock = new Mock<IRespClient>();
            var target = new RedisCache(new RedisCacheOptions(), respClientMock.Object);
            var expectedKey = "foo";
            var expectedValue = RespProtocol.Encoding.GetBytes("bar");
            var expectedExpiration = TimeSpan.FromSeconds(50);

            target.Set(expectedKey, expectedValue, new DistributedCacheEntryOptions { SlidingExpiration = expectedExpiration });

            IRequest[] requests = null;
            Func<IEnumerable<IRequest>, bool> assert = r =>
            {
                requests = r.ToArray();
                return r != null;
            };
            respClientMock.Verify(c => c.Execute(It.Is<IEnumerable<IRequest>>(r => assert(r))), Times.Once());
            Assert.AreEqual("HMSET", requests[0].Command);
            Assert.AreEqual(5, requests[0].GetArgs().Length);
            Assert.AreEqual(expectedKey, requests[0].GetArg<string>(0));
            CollectionAssert.AreEqual(RespProtocol.Encoding.GetBytes("val"), requests[0].GetArg<byte[]>(1));
            CollectionAssert.AreEqual(expectedValue, requests[0].GetArg<byte[]>(2));
            CollectionAssert.AreEqual(RespProtocol.Encoding.GetBytes("sld"), requests[0].GetArg<byte[]>(3));
            Assert.AreEqual(expectedExpiration, requests[0].GetArg<TimeSpan>(4));
            Assert.AreEqual("PEXPIRE", requests[1].Command);
            Assert.AreEqual(2, requests[1].GetArgs().Length);
            Assert.AreEqual(expectedKey, requests[1].GetArg<string>(0));
            Assert.AreEqual(expectedExpiration, requests[1].GetArg<TimeSpan>(1));
        }

        [TestMethod]
        public void Set_KeyValueAndAbsoluteExpirationRelativeToNow_InvokeExecute()
        {
            var respClientMock = new Mock<IRespClient>();
            var target = new RedisCache(new RedisCacheOptions(), respClientMock.Object);
            var expectedKey = "foo";
            var expectedValue = RespProtocol.Encoding.GetBytes("bar");
            var expectedExpiration = TimeSpan.FromSeconds(50);

            target.Set(expectedKey, expectedValue, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expectedExpiration });

            IRequest[] requests = null;
            Func<IEnumerable<IRequest>, bool> assert = r =>
            {
                requests = r.ToArray();
                return r != null;
            };
            respClientMock.Verify(c => c.Execute(It.Is<IEnumerable<IRequest>>(r => assert(r))), Times.Once());
            Assert.AreEqual("HMSET", requests[0].Command);
            Assert.AreEqual(5, requests[0].GetArgs().Length);
            Assert.AreEqual(expectedKey, requests[0].GetArg<string>(0));
            CollectionAssert.AreEqual(RespProtocol.Encoding.GetBytes("val"), requests[0].GetArg<byte[]>(1));
            CollectionAssert.AreEqual(expectedValue, requests[0].GetArg<byte[]>(2));
            CollectionAssert.AreEqual(RespProtocol.Encoding.GetBytes("sld"), requests[0].GetArg<byte[]>(3));
            Assert.AreEqual(TimeSpan.Zero, requests[0].GetArg<TimeSpan>(4));
            Assert.AreEqual("PEXPIRE", requests[1].Command);
            Assert.AreEqual(2, requests[1].GetArgs().Length);
            Assert.AreEqual(expectedKey, requests[1].GetArg<string>(0));
            Assert.AreEqual(expectedExpiration, requests[1].GetArg<TimeSpan>(1));
        }

        [TestMethod]
        public async Task SetAsync_KeyIsNull_Throws()
        {
            var target = new RedisCache(new RedisCacheOptions(), Mock.Of<IRespClient>());

            var e = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => target.SetAsync(null, new byte[] { 65 }, new DistributedCacheEntryOptions()));
            Assert.AreEqual(e.ParamName, "key");
        }

        [TestMethod]
        public async Task SetAsync_KeyIsEmpty_Throws()
        {
            var target = new RedisCache(new RedisCacheOptions(), Mock.Of<IRespClient>());

            var e = await Assert.ThrowsExceptionAsync<ArgumentException>(() => target.SetAsync(string.Empty, new byte[] { 65 }, new DistributedCacheEntryOptions()));
            Assert.AreEqual(e.ParamName, "key");
        }

        [TestMethod]
        public async Task SetAsync_ValueIsNull_Throws()
        {
            var target = new RedisCache(new RedisCacheOptions(), Mock.Of<IRespClient>());

            var e = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => target.SetAsync("foo", null, new DistributedCacheEntryOptions()));
            Assert.AreEqual(e.ParamName, "value");
        }

        [TestMethod]
        public async Task SetAsync_OptionsIsNull_Throws()
        {
            var target = new RedisCache(new RedisCacheOptions(), Mock.Of<IRespClient>());

            var e = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => target.SetAsync("foo", new byte[] { 65 }, null));
            Assert.AreEqual(e.ParamName, "options");
        }

        [TestMethod]
        public async Task SetAsync_KeyAndValue_InvokeExecuteAsync()
        {
            var respClientMock = new Mock<IRespClient>();

            var target = new RedisCache(new RedisCacheOptions(), respClientMock.Object);
            var expectedKey = "foo";
            var expectedValue = RespProtocol.Encoding.GetBytes("bar");

            await target.SetAsync(expectedKey, expectedValue, new DistributedCacheEntryOptions());

            IRequest request = null;
            Func<IRequest, bool> assert = r =>
            {
                request = r;
                return r != null;
            };
            respClientMock.Verify(c => c.ExecuteAsync(It.Is<IRequest>(r => assert(r)), It.IsAny<CancellationToken>()), Times.Once());
            Assert.AreEqual("HMSET", request.Command);
            Assert.AreEqual(5, request.GetArgs().Length);
            Assert.AreEqual(expectedKey, request.GetArg<string>(0));
            CollectionAssert.AreEqual(RespProtocol.Encoding.GetBytes("val"), request.GetArg<byte[]>(1));
            CollectionAssert.AreEqual(expectedValue, request.GetArg<byte[]>(2));
            CollectionAssert.AreEqual(RespProtocol.Encoding.GetBytes("sld"), request.GetArg<byte[]>(3));
            Assert.AreEqual(TimeSpan.Zero, request.GetArg<TimeSpan>(4));
        }

        [TestMethod]
        public async Task SetAsync_KeyValueAndAbsoluteExpiration_InvokeExecuteAsync()
        {
            var respClientMock = new Mock<IRespClient>();
            var target = new RedisCache(new RedisCacheOptions(), respClientMock.Object);
            var expectedKey = "foo";
            var expectedValue = RespProtocol.Encoding.GetBytes("bar");
            var now = DateTimeOffset.Now;
            var expectedExpiration = now.AddHours(1);

            await target.SetAsync(expectedKey, expectedValue, new DistributedCacheEntryOptions { AbsoluteExpiration = expectedExpiration });

            IRequest[] requests = null;
            Func<IEnumerable<IRequest>, bool> assert = r =>
            {
                requests = r.ToArray();
                return r != null;
            };
            respClientMock.Verify(c => c.ExecuteAsync(It.Is<IEnumerable<IRequest>>(r => assert(r)), It.IsAny<CancellationToken>()), Times.Once());
            Assert.AreEqual("HMSET", requests[0].Command);
            Assert.AreEqual(5, requests[0].GetArgs().Length);
            Assert.AreEqual(expectedKey, requests[0].GetArg<string>(0));
            CollectionAssert.AreEqual(RespProtocol.Encoding.GetBytes("val"), requests[0].GetArg<byte[]>(1));
            CollectionAssert.AreEqual(expectedValue, requests[0].GetArg<byte[]>(2));
            CollectionAssert.AreEqual(RespProtocol.Encoding.GetBytes("sld"), requests[0].GetArg<byte[]>(3));
            Assert.AreEqual(TimeSpan.Zero, requests[0].GetArg<TimeSpan>(4));
            Assert.AreEqual("PEXPIREAT", requests[1].Command);
            Assert.AreEqual(2, requests[1].GetArgs().Length);
            Assert.AreEqual(expectedKey, requests[1].GetArg<string>(0));
            Assert.AreEqual(expectedExpiration, requests[1].GetArg<DateTimeOffset>(1));
        }

        [TestMethod]
        public async Task SetAsync_KeyValueAndSlidingExpiration_InvokeExecuteAsync()
        {
            var respClientMock = new Mock<IRespClient>();
            var target = new RedisCache(new RedisCacheOptions(), respClientMock.Object);
            var expectedKey = "foo";
            var expectedValue = RespProtocol.Encoding.GetBytes("bar");
            var expectedExpiration = TimeSpan.FromSeconds(50);

            await target.SetAsync(expectedKey, expectedValue, new DistributedCacheEntryOptions { SlidingExpiration = expectedExpiration });

            IRequest[] requests = null;
            Func<IEnumerable<IRequest>, bool> assert = r =>
            {
                requests = r.ToArray();
                return r != null;
            };
            respClientMock.Verify(c => c.ExecuteAsync(It.Is<IEnumerable<IRequest>>(r => assert(r)), It.IsAny<CancellationToken>()), Times.Once());
            Assert.AreEqual("HMSET", requests[0].Command);
            Assert.AreEqual(5, requests[0].GetArgs().Length);
            Assert.AreEqual(expectedKey, requests[0].GetArg<string>(0));
            CollectionAssert.AreEqual(RespProtocol.Encoding.GetBytes("val"), requests[0].GetArg<byte[]>(1));
            CollectionAssert.AreEqual(expectedValue, requests[0].GetArg<byte[]>(2));
            CollectionAssert.AreEqual(RespProtocol.Encoding.GetBytes("sld"), requests[0].GetArg<byte[]>(3));
            Assert.AreEqual(expectedExpiration, requests[0].GetArg<TimeSpan>(4));
            Assert.AreEqual("PEXPIRE", requests[1].Command);
            Assert.AreEqual(2, requests[1].GetArgs().Length);
            Assert.AreEqual(expectedKey, requests[1].GetArg<string>(0));
            Assert.AreEqual(expectedExpiration, requests[1].GetArg<TimeSpan>(1));
        }

        [TestMethod]
        public async Task SetAsync_KeyValueAndAbsoluteExpirationRelativeToNow_InvokeExecuteAsync()
        {
            var respClientMock = new Mock<IRespClient>();
            var target = new RedisCache(new RedisCacheOptions(), respClientMock.Object);
            var expectedKey = "foo";
            var expectedValue = RespProtocol.Encoding.GetBytes("bar");
            var expectedExpiration = TimeSpan.FromSeconds(50);

            await target.SetAsync(expectedKey, expectedValue, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expectedExpiration });

            IRequest[] requests = null;
            Func<IEnumerable<IRequest>, bool> assert = r =>
            {
                requests = r.ToArray();
                return r != null;
            };
            respClientMock.Verify(c => c.ExecuteAsync(It.Is<IEnumerable<IRequest>>(r => assert(r)), It.IsAny<CancellationToken>()), Times.Once());
            Assert.AreEqual("HMSET", requests[0].Command);
            Assert.AreEqual(5, requests[0].GetArgs().Length);
            Assert.AreEqual(expectedKey, requests[0].GetArg<string>(0));
            CollectionAssert.AreEqual(RespProtocol.Encoding.GetBytes("val"), requests[0].GetArg<byte[]>(1));
            CollectionAssert.AreEqual(expectedValue, requests[0].GetArg<byte[]>(2));
            CollectionAssert.AreEqual(RespProtocol.Encoding.GetBytes("sld"), requests[0].GetArg<byte[]>(3));
            Assert.AreEqual(TimeSpan.Zero, requests[0].GetArg<TimeSpan>(4));
            Assert.AreEqual("PEXPIRE", requests[1].Command);
            Assert.AreEqual(2, requests[1].GetArgs().Length);
            Assert.AreEqual(expectedKey, requests[1].GetArg<string>(0));
            Assert.AreEqual(expectedExpiration, requests[1].GetArg<TimeSpan>(1));
        }
    }
}